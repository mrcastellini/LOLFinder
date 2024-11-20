using HtmlAgilityPack;
using System;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Collections;

class Program
{
    static async Task Main(string[] args)
    {
        string playerName = "";
        string playerID = "";

        // 1. Solicitar o nickname e o ID do jogador
        while (string.IsNullOrWhiteSpace(playerName))
        {
            Console.Write("Digite o nickname do jogador de League of Legends: ");
            playerName = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(playerName))
            {
                Console.WriteLine("Por favor, insira um nickname válido.");
            }
        }

        while (string.IsNullOrWhiteSpace(playerID))
        {
            Console.Write("Digite o ID do jogador de League of Legends: ");
            playerID = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(playerID))
            {
                Console.WriteLine("Por favor, insira um ID válido.");
            }
        }

        try
        {
            // 2. Fazer a requisição HTTP para o site leagueofgraphs.com
            var playerUrl = $"https://www.leagueofgraphs.com/summoner/br/{playerName.Replace(" ", "+")}-{playerID}";
            Console.WriteLine("Acessando dados do jogador pela URL {0}", playerUrl);
            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");


            var response = await client.GetStringAsync(playerUrl);

            // 3. Fazer o parsing do HTML da página
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(response);

            // 4. Selecionar todos os nós com as classes e tags determinadas abaixo na página
            var victoryNodes = htmlDoc.DocumentNode.SelectNodes("//td[contains(@class, 'resultCellLight text-center')]//div[contains(@class, 'victoryDefeatText victory')]");
            var defeatNodes = htmlDoc.DocumentNode.SelectNodes("//td[contains(@class, 'resultCellLight text-center')]//div[contains(@class, 'victoryDefeatText defeat')]");
            var surrenderNode = htmlDoc.DocumentNode.SelectSingleNode("//progressbar[@data-color='wgyellow']");
            var championNode = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'txt')]//div[contains(@class, 'name')]");

            if (surrenderNode != null)
            {
                var surrenderRate = surrenderNode.GetAttributeValue("data-value", "0");

                double surrenderRateDouble = double.Parse(surrenderRate); // Conversão do dado obtido em valor decimal

                if (championNode != null)
                {
                    string championName = championNode.InnerText.Trim();

                    var counterUrl = $"https://www.op.gg/champions/{championName.ToLower()}/counters"; // URL que inclui o nome do campeão
                    var counterResponse = await client.GetStringAsync(counterUrl);

                    // 4.1. Fazer o parsing do HTML da página de counters
                    var counterDoc = new HtmlDocument();
                    counterDoc.LoadHtml(counterResponse);

                    // 4.2. Buscar os counters do campeão mais jogado
                    var countersNode = counterDoc.DocumentNode.SelectNodes("//div[contains(@class, 'css-72rvq0 ezvw2kd4')]");
                    if (countersNode != null && countersNode.Any())
                    {

                        // 4.2.1. Verificar e contar as ocorrências
                        if (victoryNodes != null)
                        {
                            // 4.2.1.1. Contar quantos elementos com as classes buscadas acimas que foram encontrados
                            int victoryCount = victoryNodes.Count;
                            if (defeatNodes != null)
                            {
                                int defeatCount = defeatNodes.Count;

                                // 4.2.1.1.1. Contar o número de recriações de partidas baseada nos valores de contagem de vitórias e derrotas
                                int remakeCount = victoryCount + defeatCount;
                                if (remakeCount < 10)
                                {
                                    remakeCount = victoryCount - defeatCount;
                                    if (remakeCount < 0)
                                    {
                                        remakeCount = remakeCount * -1;
                                    }
                                }
                                else
                                {
                                    remakeCount = 0;
                                }

                                Console.WriteLine($"O jogador {playerName} tem {victoryCount} vitória(s), {defeatCount} derrota(s) e {remakeCount} recriação(ões) de partida(s) acumuladas nas últimas 10 partidas.");
                                if (surrenderRateDouble > 50000000000000 && surrenderRateDouble < 1000000000000000)
                                {

                                    double surrenderRatePercentage = surrenderRateDouble / 10000000000000; // Conversão do dado obtido em porcentagem
                                    Console.WriteLine($"A taxa de rendição de {playerName} é de {surrenderRatePercentage:F2}%!");
                                }
                                else
                                {
                                    double surrenderRatePercentage = surrenderRateDouble / 1000000000000; // Conversão do dado obtido em porcentagem
                                    Console.WriteLine($"A taxa de rendição de {playerName} é de {surrenderRatePercentage:F2}%!");
                                }
                                Console.Write($"Digite aqui o nome do campeão com o qual {playerName} está jogando agora: ");
                                string championPicked = Console.ReadLine();
                                if (championNode != null)
                                {
                                    championPicked = championPicked;

                                    if (championPicked == "Wukong")
                                    {
                                        championPicked = "monkeyking";
                                    }
                                        var pickedUrl = $"https://www.op.gg/champions/{championPicked.ToLower()}/counters"; // URL que inclui o nome do campeão
                                        var pickedResponse = await client.GetStringAsync(pickedUrl);

                                        var pickedDoc = new HtmlDocument();
                                        pickedDoc.LoadHtml(pickedResponse);

                                        var pickedNode = pickedDoc.DocumentNode.SelectNodes("//div[contains(@class, 'css-72rvq0 ezvw2kd4')]");
                                        if (pickedNode != null && pickedNode.Any())
                                        {
                                            if (championPicked == "monkeyking")
                                            {
                                                championPicked = "Wukong";
                                            }
                                            else
                                            {
                                                championPicked = championPicked;
                                            }
                                            Console.Write("Os melhores campeões para se selecionar contra " + championPicked + " são: | ");
                                            int loop = 0;
                                            foreach (var picked in pickedNode)
                                            {
                                                loop++;
                                                string pickedCounterChampion = picked.InnerText.Trim();

                                                // Aqui, você pode adicionar um filtro, caso queira procurar por um nome específico ou filtrar os counters.
                                                // Por exemplo, se você souber que um campeão específico é o counter que você procura.

                                                Console.Write($"{pickedCounterChampion.Replace("&#x27;", "'").Replace("&amp;", "&")} | ");
                                                if (loop >= 5)
                                                {
                                                    break;
                                                }
                                            }
                                            Console.WriteLine();
                                            Console.WriteLine($"{playerName} ama jogar de {championName}!");
                                            Console.Write("Os melhores campeões para se selecionar contra " + championName + " são: | ");
                                            int loopPicked = 0;
                                            foreach (var counter in countersNode)
                                            {
                                                loopPicked++;
                                                string counterChampion = counter.InnerText.Trim();

                                                // Aqui, você pode adicionar um filtro, caso queira procurar por um nome específico ou filtrar os counters.
                                                // Por exemplo, se você souber que um campeão específico é o counter que você procura.

                                                Console.Write($"{counterChampion.Replace("&#x27;", "'").Replace("&amp;", "&")} | ");
                                                if (loopPicked >= 5)
                                                {
                                                    break;
                                                }
                                            }

                                            string[] dicas = new string[]
                        {
            "Dica: Sempre verifique o mapa para evitar emboscadas.",
            "Dica: Use as runas e builds adequadas para o seu campeão.",
            "Dica: A visão é fundamental. Coloque sentinelas para evitar surpresas.",
            "Dica: Não ignore os dragões e barões, eles podem mudar o jogo.",
            "Dica: Aprenda a usar as habilidades do seu campeão no momento certo.",
            "Dica: Tenha paciência no início do jogo, não tente fazer tudo sozinho.",
            "Dica: O trabalho em equipe é essencial para uma vitória, coordene com seus aliados!",
            "Dica: Não se esqueça de comprar itens apropriados durante a partida."
                        };
                                            Random rand = new Random();
                                            int indiceDica = rand.Next(dicas.Length);

                                            Console.WriteLine();
                                            Console.WriteLine(dicas[indiceDica]);
                                            Console.Write("Pressione a tecla ENTER para sair...");
                                            Console.ReadLine();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        catch (HttpRequestException ex)
        {
            Console.WriteLine("Jogador não localizado!");
            Console.Write("Pressione a tecla ENTER para sair...");
            Console.ReadLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro inesperado: " + ex.Message);
            Console.Write("Pressione a tecla ENTER para sair...");
            Console.ReadLine();
        }
    }
}