using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace BatalhaNavalCmd
{
    class Program
    {
        // definicoes de programa
        private static string titulo = "Jogo da Batalha Naval";
        private static string banner = " * * * Batalha Naval * * * ";
        private static string banner_scores = " * * * Top 10 High Scores! * * * ";
        private static string ficheiro_scores = "BatalhaNavalScores.txt";
        private static string mostrar_tabuleiro_pc = "false";
        private static string config_xml = "BatalhaNavalConfig.xml";


        // definicoes de jogo
        private static char indicador_porta_avioes = 'A';
        private static char indicador_battleship = 'B';
        private static char indicador_destroyer = 'D';
        private static char indicador_submarino = 'S';
        private static char indicador_tiro_na_agua = 'x';
        private static char indicador_adjacente = '\\';
        private static char indicador_agua = '~';
        private static int colunas_tabuleiro = 8;
        private static int linhas_tabuleiro = 8;
        private static int numero_posicoes_navios = 0;
        private static uint numero_navios = 0;
        private static string limites = "true";

        // dados dos jogadores
        private static string nome_jogador1 = "";
        private static string nome_jogador2 = "";

        // indicador de colunas (maximo de colunas: 15)
        private static char[] indicador_de_coluna = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O' };

        static void Main(string[] args)
        {
            InicializarPrograma();
            Apresentacao();
            MostrarMenu();
        }

        private static void MostrarMenu()
        {
            int opcao = 0;

            do
            {
                Console.Clear();
                Menu();
                string input = Console.ReadLine();
                Int32.TryParse(input, out opcao);
                switch (opcao)
                {
                    case 0:
                        MostrarAjuda();
                        break;
                    case 1: // jogador contra jogador
                        Console.Clear();
                        InicializarJogoPvp();
                        break;
                    case 2: // jogador contra pc
                        Console.Clear();
                        InicializarJogoPc();
                        break;
                    case 3: // definicoes
                        Console.Clear();
                        Definicoes();
                        break;
                    case 4: // mostrar top 10
                        Console.Clear();
                        MostrarTop10();
                        break;
                    case 5:
                        Console.Clear();
                        //InicializarOnline();
                        break;
                    case 6: // sair
                        Console.WriteLine();
                        Console.WriteLine("Adeus, volta sempre!");
                        Console.WriteLine("Prime uma tecla para terminar...");
                        Console.ReadKey();
                        break;
                    default:
                        Console.Clear();
                        Console.WriteLine("Opcao invalida.");
                        break;
                }
            } while (opcao != 6);
        }

        private static void MostrarAjuda()
        {
            Console.Clear();
            Console.WriteLine("Tabuleiros:");
            Console.WriteLine("\t6 x 6\t1 Battleship, 1 Destroyer, 1 Submarino");
            Console.WriteLine("\t8 x 8\t1 Porta Avioes, 1 Battleship, 1 Destroyer, 2 Submarinos");
            Console.WriteLine("\t8 x 12\t1 Porta Avioes, 1 Battleship, 2 Destroyers, 2 Submarinos");
            Console.WriteLine();
            Console.WriteLine("Legenda:");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("\t" + indicador_porta_avioes);
            Console.ResetColor();
            Console.WriteLine("\tPorta Avioes - 4 posicoes - 1000 pontos");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("\t" + indicador_battleship);
            Console.ResetColor();
            Console.WriteLine("\tBattleship - 3 posicoes - 600 pontos");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("\t" + indicador_destroyer);
            Console.ResetColor();
            Console.WriteLine("\tDestroyer - 2 posicoes - 200 pontos");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("\t" + indicador_submarino);
            Console.ResetColor();
            Console.WriteLine("\tSubmarino - 1 posicao - 100 pontos");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("\t" + indicador_adjacente);
            Console.ResetColor();
            Console.WriteLine("\tLimites adjacentes. Nao e permitido colocar navio.");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("\t" + indicador_tiro_na_agua);
            Console.ResetColor();
            Console.WriteLine("\tTiro acertou na agua.");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("\t" + indicador_agua);
            Console.ResetColor();
            Console.WriteLine("\tAgua.");
            Console.WriteLine();
            Console.WriteLine("Prime uma tecla para voltar ao menu...");
            Console.ReadKey();
        }

        private static void MostrarTop10()
        {
            Console.WriteLine(banner_scores);
            Console.WriteLine();
            Console.WriteLine("Posicao\tScore\tJogadas\tNome");

            if (File.Exists(ficheiro_scores))
            {
                string[] entrada = new string[3];
                StreamReader str = File.OpenText(ficheiro_scores);
                string read = null;

                for (int i = 1; (read = str.ReadLine()) != null; ++i)
                {
                    entrada = read.Split(';');
                    Console.WriteLine(i + ".\t" + entrada[0] + "\t" + entrada[1] + "\t" + entrada[2]);
                }
                str.Close();
            }
            else
            {
                Console.WriteLine("(Ainda não existem entradas...)");
            }
            Console.WriteLine();
            Console.WriteLine("Prime uma tecla para continuar...");
            Console.ReadKey();
            Console.Clear();
        }

        private static void InicializarPrograma()
        {
            if (File.Exists(config_xml))
            {
                XmlDocument config = new XmlDocument();
                config.Load(config_xml);

                titulo = config.SelectSingleNode("//local/jogo/titulo").InnerText;
                banner = config.SelectSingleNode("//local/jogo/banner").InnerText;
                banner_scores = config.SelectSingleNode("//local/jogo/banner_scores").InnerText;
                ficheiro_scores = config.SelectSingleNode("//local/jogo/ficheiro_scores").InnerText;
                colunas_tabuleiro = Convert.ToInt32(config.SelectSingleNode("//local/jogo/predefinicoes/colunas").InnerText);
                linhas_tabuleiro = Convert.ToInt32(config.SelectSingleNode("//local/jogo/predefinicoes/linhas").InnerText);
                mostrar_tabuleiro_pc = config.SelectSingleNode("//local/jogo/mostra_tabuleiro_pc").InnerText;
            }
            else
            {
                try
                {
                    StreamWriter w = new StreamWriter(config_xml);
                    w.WriteLine("<local>");
                    w.WriteLine("    <jogo>");
                    w.WriteLine("        <titulo>Jogo da Batalha Naval</titulo>");
                    w.WriteLine("        <banner> * * * Batalha Naval * * * </banner>");
                    w.WriteLine("        <banner_scores> * * * Top 10 High Scores! * * * </banner_scores>");
                    w.WriteLine("        <ficheiro_scores>BatalhaNavalScores.txt</ficheiro_scores>");
                    w.WriteLine("        <mostra_tabuleiro_pc>false</mostra_tabuleiro_pc>");
                    w.WriteLine("        <predefinicoes>");
                    w.WriteLine("            <!-- colunas: valores entre 6 e 15 -->");
                    w.WriteLine("            <colunas>8</colunas>");
                    w.WriteLine("            <!-- linhas: valores entre 6 e 9 -->");
                    w.WriteLine("            <linhas>8</linhas>");
                    w.WriteLine("        </predefinicoes>");
                    w.WriteLine("    </jogo>");
                    w.WriteLine("</local>");
                    w.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine("Nao foi possivel gerar ficheiro \"" + config_xml + "\"");
                    Console.WriteLine("Prime uma tecla para continuar com as predefinicoes...");
                    Console.ReadKey();
                }
                Console.Clear();
            }
            Console.Title = titulo;
        }

        private static void Apresentacao()
        {
            int tamanho_linha = 15;

            SoundPlayer player = new SoundPlayer(Properties.Resources.musica1);
            player.PlayLooping();

            for (int i = 0; !Console.KeyAvailable; ++i)
            {
                Console.WriteLine("Copyright (c) 2017 Paulo Pocinho");
                Console.WriteLine();
                Console.WriteLine(banner);
                Console.WriteLine();
                Console.WriteLine("              __");
                Console.WriteLine("              \\ \\___     .__");
                Console.WriteLine("            .--\"\"___\\..--\"/");
                Console.WriteLine("        .__.|-\"\"\"..... ' /");
                Console.WriteLine("________\\_______________/___________");

                if (i % 2 == 0)
                {
                    for (int j = 0; j < tamanho_linha; ++j)
                    {
                        if (j % 2 == 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.Write(" " + indicador_agua + " ");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.Write(" " + indicador_agua + " ");
                        }
                    }
                    Console.WriteLine();
                    Console.Write("  ");
                    for (int j = 0; j < tamanho_linha; ++j)
                    {
                        if (j % 2 == 0)
                        {
                            Console.Write(" " + indicador_agua + " ");
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.Write(" " + indicador_agua + " ");
                            Console.ResetColor();
                        }
                    }
                    Console.WriteLine();
                }
                else
                {
                    for (int j = 0; j < tamanho_linha; ++j)
                    {
                        if (j % 2 == 0)
                        {
                            Console.Write(" " + indicador_agua + " ");
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.Write(" " + indicador_agua + " ");
                            Console.ResetColor();
                        }
                    }
                    Console.WriteLine();
                    Console.Write("  ");
                    for (int j = 0; j < tamanho_linha; ++j)
                    {
                        if (j % 2 == 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.Write(" " + indicador_agua + " ");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.Write(" " + indicador_agua + " ");
                        }
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();
                Console.WriteLine("Prime uma tecla...");
                Thread.Sleep(400);
                Console.Clear();
            }
            player.Stop();
        }

        private static void Menu()
        {
            Console.WriteLine("Insere opcao:");
            Console.WriteLine("\t0 - Ajuda");
            Console.WriteLine("\t1 - Jogador contra Jogador");
            Console.WriteLine("\t2 - Jogador contra PC");
            Console.WriteLine("\t3 - Definicoes");
            Console.WriteLine("\t4 - Top 10 Scores");
            //Console.WriteLine("\t5 - Jogar Online");
            Console.WriteLine("\t6 - Sair");
        }

        private static void MenuDefinicoes()
        {
            Console.WriteLine("Largura do tabuleiro: " + colunas_tabuleiro);
            Console.WriteLine("Altura do tabuleiro: " + linhas_tabuleiro);
            Console.WriteLine();
            Console.WriteLine("Insere opcao:");
            Console.WriteLine("\t1 - Escolher nova largura do tabuleiro");
            Console.WriteLine("\t2 - Escolher nova altura do tabuleiro");
            Console.WriteLine("\t3 - Voltar ao menu anterior");
        }

        private static void Definicoes()
        {
            int opcao = 0;
            string input = "";

            do
            {
                MenuDefinicoes();
                input = Console.ReadLine();
                Int32.TryParse(input, out opcao);
                switch (opcao)
                {
                    case 1: // escolher largura do tabuleiro
                        do
                        {
                            Console.WriteLine("Insere um numero entre 6 e 15");
                            input = Console.ReadLine();
                            Int32.TryParse(input, out colunas_tabuleiro);
                        } while (colunas_tabuleiro < 6 || colunas_tabuleiro > 15);
                        Console.Clear();
                        break;
                    case 2: // escolher altura do tabuleiro
                        do
                        {
                            Console.WriteLine("Insere um numero entre 6 e 9");
                            input = Console.ReadLine();
                            Int32.TryParse(input, out linhas_tabuleiro);
                        } while (linhas_tabuleiro < 6 || linhas_tabuleiro > 9);
                        Console.Clear();
                        break;
                    case 3: // voltar ao menu principal
                        Console.Clear();
                        break;
                    default:
                        Console.Clear();
                        Console.WriteLine("Opcao invalida.");
                        break;
                }
            } while (opcao != 3);
        }

        private static void InicializarTabuleiros(string[,] tabuleiro_jogador1, string[,] tabuleiro_jogador2, string[,] tabuleiro_de_jogo_jogador1, string[,] tabuleiro_de_jogo_jogador2)
        {
            /*
                 0   1   2   3
             0 |   | A | B | C ...
             1 | 1 | 
             2 | 2 | 
             ...
             */

            tabuleiro_jogador1[0, 0] = "   ";
            tabuleiro_jogador2[0, 0] = "   ";
            tabuleiro_de_jogo_jogador1[0, 0] = "   ";
            tabuleiro_de_jogo_jogador2[0, 0] = "   ";

            // preencher linha de letras
            for (int i = 1; i < colunas_tabuleiro; ++i)
            {
                tabuleiro_jogador1[0, i] = " " + indicador_de_coluna[i - 1] + " ";
                tabuleiro_jogador2[0, i] = " " + indicador_de_coluna[i - 1] + " ";
                tabuleiro_de_jogo_jogador1[0, i] = " " + indicador_de_coluna[i - 1] + " ";
                tabuleiro_de_jogo_jogador2[0, i] = " " + indicador_de_coluna[i - 1] + " ";
            }

            // preencher coluna de numeros
            for (int i = 1; i < linhas_tabuleiro; ++i)
            {
                tabuleiro_jogador1[i, 0] = " " + i + " ";
                tabuleiro_jogador2[i, 0] = " " + i + " ";
                tabuleiro_de_jogo_jogador1[i, 0] = " " + i + " ";
                tabuleiro_de_jogo_jogador2[i, 0] = " " + i + " ";
            }

            // preencher espaço de jogo
            for (int i = 1; i < linhas_tabuleiro; ++i)
            {
                for (int j = 1; j < colunas_tabuleiro; ++j)
                {
                    tabuleiro_jogador1[i, j] = " " + indicador_agua + " ";
                    tabuleiro_jogador2[i, j] = " " + indicador_agua + " ";
                    tabuleiro_de_jogo_jogador1[i, j] = " " + indicador_agua + " ";
                    tabuleiro_de_jogo_jogador2[i, j] = " " + indicador_agua + " ";
                }
            }
        }

        private static void InicializarJogoPvp()
        {
            // alocar espaco para a linha de letras e a coluna de numeros
            colunas_tabuleiro++;
            linhas_tabuleiro++;

            // tabuleiros onde os jogadores colocam os navios
            string[,] tabuleiro_jogador1 = new string[linhas_tabuleiro, colunas_tabuleiro];
            string[,] tabuleiro_jogador2 = new string[linhas_tabuleiro, colunas_tabuleiro];

            // tabuleiros de jogo onde os jogadores tentam afundar os navios do adversario
            string[,] tabuleiro_de_jogo_jogador1 = new string[linhas_tabuleiro, colunas_tabuleiro];
            string[,] tabuleiro_de_jogo_jogador2 = new string[linhas_tabuleiro, colunas_tabuleiro];

            InicializarTabuleiros(tabuleiro_jogador1, tabuleiro_jogador2, tabuleiro_de_jogo_jogador1, tabuleiro_de_jogo_jogador2);

            do
            {
                Console.Clear();
                Console.WriteLine("Jogador 1 - Insere o teu nome:");
                nome_jogador1 = Console.ReadLine();
            } while (nome_jogador1 == "");
            InicializarJogador(tabuleiro_jogador1, nome_jogador1);

            do
            {
                Console.Clear();
                Console.WriteLine("Jogador 2 - Insere o teu nome:");
                nome_jogador2 = Console.ReadLine();
            } while (nome_jogador2 == "");
            InicializarJogador(tabuleiro_jogador2, nome_jogador2);

            Console.Clear();
            JogarContraJogador(tabuleiro_jogador1, tabuleiro_jogador2, tabuleiro_de_jogo_jogador1, tabuleiro_de_jogo_jogador2);

            // reverter tamanho de tabuleiro para o valor definido
            colunas_tabuleiro--;
            linhas_tabuleiro--;
        }

        private static void JogarContraJogador(string[,] tabuleiro_jogador1, string[,] tabuleiro_jogador2, string[,] tabuleiro_de_jogo_jogador1, string[,] tabuleiro_de_jogo_jogador2)
        {
            int tentativas_jogador1 = 0;
            int tentativas_jogador2 = 0;

            int barcos_afundados_jogador1 = 0;
            int barcos_afundados_jogador2 = 0;

            Random rnd = new Random();
            int vez = rnd.Next(1, 3);

            string calcular_vez = "A calcular vez";

            bool acertou = false;
            bool fim_de_jogo = false;

            for (int i = 0; i < 4; ++i)
            {
                Console.WriteLine(calcular_vez);
                calcular_vez += ".";
                Thread.Sleep(600);
                Console.Clear();
            }

            while (!fim_de_jogo)
            {
                if (vez == 1)
                {
                    do
                    {
                        acertou = JogadaJogador(nome_jogador1, tabuleiro_de_jogo_jogador1, tabuleiro_jogador2, ref barcos_afundados_jogador1, ref tentativas_jogador1);

                        if (barcos_afundados_jogador1 == numero_posicoes_navios)
                        {
                            FimDeJogo(tabuleiro_de_jogo_jogador1, tentativas_jogador1, nome_jogador1);
                            fim_de_jogo = true;
                            break;
                        }
                        else
                        {
                            if (acertou)
                                Console.WriteLine("Prime uma tecla para continuar...");
                            else
                            {
                                vez = 2;
                                Console.WriteLine("Prime uma tecla para dar a vez...");
                            }
                            Console.ReadKey();
                        }
                    } while (acertou);
                }
                else
                {
                    do
                    {
                        acertou = JogadaJogador(nome_jogador2, tabuleiro_de_jogo_jogador2, tabuleiro_jogador1, ref barcos_afundados_jogador2, ref tentativas_jogador2);

                        if (barcos_afundados_jogador2 == numero_posicoes_navios)
                        {
                            FimDeJogo(tabuleiro_de_jogo_jogador2, tentativas_jogador2, nome_jogador2);
                            fim_de_jogo = true;
                            break;
                        }
                        else
                        {
                            if (acertou)
                                Console.WriteLine("Prime uma tecla para continuar...");
                            else
                            {
                                vez = 1;
                                Console.WriteLine("Prime uma tecla para dar a vez...");
                            }
                            Console.ReadKey();
                        }
                    } while (acertou);
                }
            }
        }

        // fim de jogo quando um Jogador ganha
        private static void FimDeJogo(string[,] tabuleiro, int tentativas, string nome_jogador)
        {
            int score_porta_avioes = VerificarPontos(tabuleiro, indicador_porta_avioes);
            int score_battleship = VerificarPontos(tabuleiro, indicador_battleship); ;
            int score_destroyer = VerificarPontos(tabuleiro, indicador_destroyer); ;
            int score_submarino = VerificarPontos(tabuleiro, indicador_submarino); ;

            int score_total = score_porta_avioes + score_battleship + score_destroyer + score_submarino;

            // Dar mais 1500 pontos por ter ganho o jogo
            score_total += 1500;

            // retirar ao score nr. de tentativas * 10
            score_total -= (tentativas * 10);

            Console.WriteLine("Parabens, " + nome_jogador + "! Ganhaste com " + score_total + " pontos!");
            Console.WriteLine();
            EscreverScores(score_total, tentativas, nome_jogador);
            Console.WriteLine("Prime uma tecla para continuar...");
            Console.ReadKey();
            Console.Clear();
            MostrarTop10();
        }

        // fim de jogo quando o pc ganha
        private static void FimDeJogoPC(string[,] tabuleiro, int tentativas, string nome_jogador)
        {
            int score_porta_avioes = VerificarPontos(tabuleiro, indicador_porta_avioes);
            int score_battleship = VerificarPontos(tabuleiro, indicador_battleship); ;
            int score_destroyer = VerificarPontos(tabuleiro, indicador_destroyer); ;
            int score_submarino = VerificarPontos(tabuleiro, indicador_submarino); ;

            int score_total = score_porta_avioes + score_battleship + score_destroyer + score_submarino;

            // retirar ao score nr. de tentativas * 10
            score_total -= (tentativas * 10);

            // nao escrever scores negativos
            if (score_total < 0)
                score_total = 0;

            Console.WriteLine("Parabens, " + nome_jogador + "! Ficaste com " + score_total + " pontos!");
            Console.WriteLine();
            EscreverScores(score_total, tentativas, nome_jogador);
            Console.WriteLine("Prime uma tecla para continuar...");
            Console.ReadKey();
            Console.Clear();
            MostrarTop10();
        }

        private static int VerificarPontos(string[,] tabuleiro, char tipo_navio)
        {
            int tiros_certeiros = 0;
            int resultado = 0;

            if (tipo_navio == indicador_porta_avioes)
            {
                for (int i = 1; i < linhas_tabuleiro; ++i)
                {
                    for (int j = 1; j < colunas_tabuleiro; ++j)
                    {
                        if (tabuleiro[i, j].Contains(tipo_navio))
                        {
                            tiros_certeiros++;
                        }
                    }
                }
                resultado = tiros_certeiros * 250;
            }
            else if (tipo_navio == indicador_battleship)
            {
                for (int i = 1; i < linhas_tabuleiro; ++i)
                {
                    for (int j = 1; j < colunas_tabuleiro; ++j)
                    {
                        if (tabuleiro[i, j].Contains(tipo_navio))
                        {
                            tiros_certeiros++;
                        }
                    }
                }
                resultado = tiros_certeiros * 200;
            }
            else if (tipo_navio == indicador_destroyer) // destroyer pode repetir-se, temos de ver se o afundámos
            {
                for (int i = 1; i < linhas_tabuleiro; ++i)
                {
                    for (int j = 1; j < colunas_tabuleiro; ++j)
                    {
                        if (tabuleiro[i, j].Contains(tipo_navio))
                        {
                            if (VerificarNavio(tabuleiro, i, j, tipo_navio))
                                resultado += 200;
                            else
                                resultado += 100;
                        }
                    }
                }
            }
            else if (tipo_navio == indicador_submarino)
            {
                for (int i = 1; i < linhas_tabuleiro; ++i)
                {
                    for (int j = 1; j < colunas_tabuleiro; ++j)
                    {
                        if (tabuleiro[i, j].Contains(tipo_navio))
                        {
                            resultado += 100;
                        }
                    }
                }
            }
            return resultado;
        }

        private static bool JogadaJogador(string nome_jogador, string[,] tabuleiro_de_jogo, string[,] tabuleiro_oponente, ref int alvos_atingidos, ref int tentativas)
        {
            string input = "";
            int linha = 0;
            char letra = '\0';
            int coluna = 0;
            bool verifica_letra = true;
            bool acertou = false;

            string tipo_de_barco_atingido = "";

            do
            {
                Console.Clear();
                Console.WriteLine(nome_jogador + ", é a tua vez de acertar nos navios!");
                Console.WriteLine();
                ImprimirTabuleiro(tabuleiro_de_jogo);
                Console.WriteLine();
                Console.WriteLine("Tentativas: " + tentativas);
                Console.WriteLine("Alvos atingidos: " + alvos_atingidos);
                Console.WriteLine();
                do
                {
                    Console.WriteLine("Insere o numero da posicao:");
                    input = Console.ReadLine();
                    Int32.TryParse(input, out linha);
                } while (linha < 1 || linha >= linhas_tabuleiro);

                while (verifica_letra || coluna >= colunas_tabuleiro)
                {
                    Console.WriteLine("Insere a letra da posicao:");
                    input = Console.ReadLine().ToUpper();
                    Char.TryParse(input, out letra);
                    for (int k = 0; k < colunas_tabuleiro; ++k)
                    {
                        if (letra == indicador_de_coluna[k])
                        {
                            coluna = k + 1;
                            verifica_letra = false;
                            break;
                        }
                    }
                }
                verifica_letra = true;
            } while (!tabuleiro_de_jogo[linha, coluna].Contains(indicador_agua));

            tentativas++;

            if (tabuleiro_oponente[linha, coluna].Contains(indicador_porta_avioes))
            {
                tipo_de_barco_atingido = " " + indicador_porta_avioes + " ";
                acertou = true;
            }
            else if (tabuleiro_oponente[linha, coluna].Contains(indicador_battleship))
            {
                tipo_de_barco_atingido = " " + indicador_battleship + " ";
                acertou = true;
            }
            else if (tabuleiro_oponente[linha, coluna].Contains(indicador_destroyer))
            {
                tipo_de_barco_atingido = " " + indicador_destroyer + " ";
                acertou = true;
            }
            else if (tabuleiro_oponente[linha, coluna].Contains(indicador_submarino))
            {
                tipo_de_barco_atingido = " " + indicador_submarino + " ";
                acertou = true;
            }

            Console.Clear();
            if (acertou)
            {
                SoundPlayer player = new SoundPlayer(Properties.Resources.Depth_Charge);
                player.Play();
                tabuleiro_de_jogo[linha, coluna] = tipo_de_barco_atingido;
                alvos_atingidos++;
                Console.WriteLine(nome_jogador + ", acertaste! Podes continuar.");
            }
            else
            {
                tabuleiro_de_jogo[linha, coluna] = " " + indicador_tiro_na_agua + " ";
                Console.WriteLine(nome_jogador + ", meteste agua!");
            }

            Console.WriteLine();
            ImprimirTabuleiro(tabuleiro_de_jogo);
            Console.WriteLine();
            Console.WriteLine("Tentativas: " + tentativas);
            Console.WriteLine("Alvos atingidos: " + alvos_atingidos);
            Console.WriteLine();

            return acertou;
        }

        private static List<Score> LerScores()
        {
            List<Score> top10 = new List<Score>();

            if (File.Exists(ficheiro_scores))
            {
                string[] entrada = new string[3];
                Score score_jogador;
                StreamReader str = File.OpenText(ficheiro_scores);
                string read = null;

                while ((read = str.ReadLine()) != null)
                {
                    entrada = read.Split(';');
                    score_jogador = new Score(Convert.ToInt32(entrada[0]), Convert.ToInt32(entrada[1]), entrada[2]);
                    top10.Add(score_jogador);
                }
                str.Close();
            }
            return top10;
        }

        private static void EscreverScores(int score, int tentativas, string nome)
        {

            List<Score> top10 = LerScores();

            top10.Add(new Score(score, tentativas, nome));

            OrdenarScores(top10);

            //limitar ao top 10 de high scores
            if (top10.Count > 10)
                top10.RemoveRange(10, top10.Count - 10);

            FileInfo fi = new FileInfo(ficheiro_scores);
            StreamWriter w = fi.CreateText();
            try
            {
                foreach (Score s in top10)
                    w.WriteLine(s);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Nao foi possivel escrever no ficheiro \"" + ficheiro_scores + "\"");
                Console.WriteLine("Prime uma tecla para continuar...");
                Console.ReadKey();
            }
            finally
            {
                w.Close();
            }
        }

        private static void OrdenarScores(List<Score> scores)  // BubbleSort
        {
            Score temp;
            for (int i = scores.Count - 1; i >= 0; --i)
            {
                for (int j = 0; j < i; ++j)
                {
                    if (scores[j].ScoreTotal < scores[j + 1].ScoreTotal)
                    {
                        temp = scores[j];
                        scores[j] = scores[j + 1];
                        scores[j + 1] = temp;
                    }
                }
            }
        }

        private static void InicializarJogador(string[,] tabuleiro, string jogador)
        {
            numero_posicoes_navios = 0;
            numero_navios = 0;

            // colocar mais navios de acordo com o tamanho da grelha

            if (linhas_tabuleiro > 7 && colunas_tabuleiro > 7)
            {
                // inicializar porta avioes
                PreencherJogador(4, tabuleiro, jogador);
                numero_posicoes_navios += 4;
                numero_navios++;
            }

            // inicializar Battle Ship
            PreencherJogador(3, tabuleiro, jogador);
            numero_posicoes_navios += 3;
            numero_navios++;

            // Colocar um destroyer extra
            if (linhas_tabuleiro > 7 && colunas_tabuleiro > 11)
            {
                PreencherJogador(2, tabuleiro, jogador);
                numero_posicoes_navios += 2;
                numero_navios++;
            }

            // inicializar Destroyer
            PreencherJogador(2, tabuleiro, jogador);
            numero_posicoes_navios += 2;
            numero_navios++;

            // Colocar um submarino extra
            if (linhas_tabuleiro > 7 && colunas_tabuleiro > 7)
            {
                // inicializar Submarino
                PreencherJogador(1, tabuleiro, jogador);
                numero_posicoes_navios += 1;
                numero_navios++;
            }

            // inicializar Submarino
            PreencherJogador(1, tabuleiro, jogador);
            numero_posicoes_navios += 1;
            numero_navios++;
        }

        private static void PreencherJogador(int posicoes, string[,] tabuleiro, string jogador)
        {
            string input = "";
            char letra = '\0';
            int linha_inicial = 0;
            int coluna_inicial = 0;
            int linha_final = 0;
            int coluna_final = 0;
            bool verifica_letra = true;
            bool posicao_valida = false;
            string nome_navio = "";

            switch (posicoes)
            {
                case 1:
                    nome_navio = "Submarino";
                    break;
                case 2:
                    nome_navio = "Destroyer";
                    break;
                case 3:
                    nome_navio = "Battleship";
                    break;
                case 4:
                    nome_navio = "Porta Avioes";
                    break;
                default:
                    nome_navio = "Inesperado";
                    break;
            }

            if (posicoes == 1)
            {
                do
                {
                    Console.Clear();
                    Console.WriteLine(jogador + ", preenche a grelha com " + nome_navio + " (" + posicoes + " posicoes):");
                    Console.WriteLine();
                    ImprimirTabuleiro(tabuleiro);
                    Console.WriteLine();
                    do
                    {
                        Console.WriteLine("Insere o numero da posicao para o " + nome_navio + ":");
                        input = Console.ReadLine();
                        Int32.TryParse(input, out linha_inicial);
                    } while (linha_inicial < 1 || linha_inicial >= linhas_tabuleiro);

                    while (verifica_letra || coluna_inicial >= colunas_tabuleiro)
                    {
                        Console.WriteLine("Insere a letra da posicao para o " + nome_navio + ":");
                        input = Console.ReadLine().ToUpper();
                        Char.TryParse(input, out letra);
                        for (int k = 0; k < colunas_tabuleiro; ++k)
                        {
                            if (letra == indicador_de_coluna[k])
                            {
                                coluna_inicial = k + 1;
                                verifica_letra = false;
                                break;
                            }
                        }
                    }
                    verifica_letra = true;
                    posicao_valida = VerificarPosicao(tabuleiro, linha_inicial, coluna_inicial, linha_inicial, coluna_inicial, posicoes, limites);
                } while (!posicao_valida);
                // ! TESTAR !
                MarcarPosicoes(tabuleiro, linha_inicial, coluna_inicial, linha_inicial, coluna_inicial, posicoes);
            }
            else if (posicoes > 1)
            {
                // primeira coordenada
                do
                {
                    Console.Clear();
                    Console.WriteLine(jogador + ", preenche a grelha com " + nome_navio + " (" + posicoes + " posicoes):");
                    Console.WriteLine();
                    ImprimirTabuleiro(tabuleiro);
                    Console.WriteLine();
                    do
                    {
                        Console.WriteLine("Insere o numero da posicao inicial para o " + nome_navio + ":");
                        input = Console.ReadLine();
                        Int32.TryParse(input, out linha_inicial);
                    } while (linha_inicial < 1 || linha_inicial >= linhas_tabuleiro);

                    while (verifica_letra || coluna_inicial >= colunas_tabuleiro)
                    {
                        Console.WriteLine("Insere a letra da posicao inicial para o " + nome_navio + ":");
                        input = Console.ReadLine().ToUpper();
                        Char.TryParse(input, out letra);
                        for (int k = 0; k < colunas_tabuleiro; ++k)
                        {
                            if (letra == indicador_de_coluna[k])
                            {
                                coluna_inicial = k + 1;
                                verifica_letra = false;
                                break;
                            }
                        }
                    }

                    verifica_letra = true;

                    // segunda coordenada
                    do
                    {
                        Console.WriteLine("Insere o numero da posicao final para o " + nome_navio + ":");
                        input = Console.ReadLine();
                        Int32.TryParse(input, out linha_final);
                    } while (linha_final < 1 || linha_final >= linhas_tabuleiro);

                    while (verifica_letra || coluna_final >= colunas_tabuleiro)
                    {
                        Console.WriteLine("Insere a letra da posicao final para o " + nome_navio + ":");
                        input = Console.ReadLine().ToUpper();
                        Char.TryParse(input, out letra);
                        for (int k = 0; k < colunas_tabuleiro; ++k)
                        {
                            if (letra == indicador_de_coluna[k])
                            {
                                coluna_final = k + 1;
                                verifica_letra = false;
                                break;
                            }
                        }
                    }
                    verifica_letra = true;
                    posicao_valida = VerificarPosicao(tabuleiro, linha_inicial, coluna_inicial, linha_final, coluna_final, posicoes, limites);

                    if (!posicao_valida)
                    {
                        Console.WriteLine("Posicao invalida.");
                        Console.WriteLine("Prime uma tecla para introduzir de novo as coordenadas...");
                        Console.ReadKey();
                    }

                } while (!posicao_valida);
            }

            // Marcar posicoes na grelha
            MarcarPosicoes(tabuleiro, linha_inicial, coluna_inicial, linha_final, coluna_final, posicoes);
        }

        private static void MarcarPosicoes(string[,] tabuleiro, int linha_inicial, int coluna_inicial, int linha_final, int coluna_final, int posicoes)
        {
            string tipo_navio = "";

            switch (posicoes)
            {
                case 1:
                    tipo_navio = " " + indicador_submarino + " ";
                    break;
                case 2:
                    tipo_navio = " " + indicador_destroyer + " ";
                    break;
                case 3:
                    tipo_navio = " " + indicador_battleship + " ";
                    break;
                case 4:
                    tipo_navio = " " + indicador_porta_avioes + " ";
                    break;
                default:
                    tipo_navio = "ERR";
                    break;
            }

            // Caso simples: 1 posicao
            if (posicoes == 1)
            {
                tabuleiro[linha_inicial, coluna_inicial] = tipo_navio;
            }
            // outras posicoes:
            // preencher na vertical:
            else if (coluna_inicial == coluna_final)
            {
                // preencher de baixo para cima:
                if (linha_inicial - linha_final > 0)
                {
                    for (int i = 0; i < posicoes; ++i)
                    {
                        tabuleiro[linha_inicial - i, coluna_inicial] = tipo_navio;
                    }

                }
                // preencher de cima para baixo:
                else if (linha_inicial - linha_final < 0)
                {
                    for (int i = 0; i < posicoes; ++i)
                    {
                        tabuleiro[linha_inicial + i, coluna_inicial] = tipo_navio;
                    }
                }
            }
            // preencher na horizontal:
            else if (linha_inicial == linha_final)
            {
                // preencher da esquerda para a direita:
                if (coluna_inicial - coluna_final < 0)
                {
                    for (int i = 0; i < posicoes; ++i)
                    {
                        tabuleiro[linha_inicial, coluna_inicial + i] = tipo_navio;
                    }
                }
                // preencher da direita para a esquerda:
                else if (coluna_inicial - coluna_final > 0)
                {
                    for (int i = 0; i < posicoes; ++i)
                    {
                        tabuleiro[linha_inicial, coluna_inicial - i] = tipo_navio;
                    }
                }
            }
        }

        private static bool VerificarPosicao(string[,] tabuleiro, int linha_inicial, int coluna_inicial, int linha_final, int coluna_final, int posicoes, string limites)
        {
            bool resultado = false;

            // caso simples, as posicoes não estão preenchidas
            if (tabuleiro[linha_inicial, coluna_inicial].Contains(indicador_agua) && tabuleiro[linha_final, coluna_final].Contains(indicador_agua))
            {
                // verificar na vertical:
                if (coluna_inicial == coluna_final)
                {
                    // verificar de baixo para cima:
                    if (linha_inicial - linha_final >= 0 && ((linha_inicial - posicoes + 1) == linha_final))
                    {
                        // verificar intermedios
                        bool livre = true;
                        for (int i = 1; i < posicoes; ++i)
                        {
                            if (!tabuleiro[linha_inicial - i, coluna_inicial].Contains(indicador_agua))
                            {
                                livre = false;
                                break;
                            }
                        }

                        if (livre)
                        {
                            MarcarLimites(tabuleiro, linha_inicial, coluna_inicial, linha_final, coluna_final, posicoes, "BaixoCima");
                            resultado = true;
                        }
                    }
                    // verificar de cima para baixo:
                    else if (linha_inicial - linha_final <= 0 && ((linha_inicial + posicoes - 1) == linha_final))
                    {
                        // verificar intermedios
                        bool livre = true;
                        for (int i = 1; i < posicoes; ++i)
                        {
                            if (!tabuleiro[linha_inicial + i, coluna_inicial].Contains(indicador_agua))
                            {
                                livre = false;
                                break;
                            }
                        }

                        if (livre)
                        {
                            MarcarLimites(tabuleiro, linha_inicial, coluna_inicial, linha_final, coluna_final, posicoes, "CimaBaixo");
                            resultado = true;
                        }
                    }
                }
                // verificar na horizontal:
                else if (linha_inicial == linha_final)
                {
                    // verificar da esquerda para a direita:
                    if (coluna_inicial - coluna_final <= 0 && ((coluna_inicial + posicoes - 1) == coluna_final))
                    {
                        // verificar intermedios
                        bool livre = true;
                        for (int i = 1; i < posicoes; ++i)
                        {
                            if (!tabuleiro[linha_inicial, coluna_inicial + i].Contains(indicador_agua))
                            {
                                livre = false;
                                break;
                            }
                        }

                        if (livre)
                        {
                            MarcarLimites(tabuleiro, linha_inicial, coluna_inicial, linha_final, coluna_final, posicoes, "EsqDir");
                            resultado = true;
                        }
                    }
                    // verificar da direita para a esquerda:
                    else if (coluna_inicial - coluna_final >= 0 && ((coluna_inicial - posicoes + 1) == coluna_final))
                    {
                        // verificar intermedios
                        bool livre = true;
                        for (int i = 1; i < posicoes; ++i)
                        {
                            if (!tabuleiro[linha_inicial, coluna_inicial - i].Contains(indicador_agua))
                            {
                                livre = false;
                                break;
                            }
                        }

                        if (livre)
                        {
                            MarcarLimites(tabuleiro, linha_inicial, coluna_inicial, linha_final, coluna_final, posicoes, "DirEsq");
                            resultado = true;
                        }
                    }
                }
            }

            return resultado;
        }

        private static void MarcarLimites(string[,] tabuleiro, int linha_inicial, int coluna_inicial, int linha_final, int coluna_final, int posicoes, string orientacao)
        {
            if (orientacao == "BaixoCima") // vertical
            {
                // verificar no canto superior esquerdo
                if (linha_final == 1 && coluna_final == 1)
                {
                    // marcar 1 abaixo e N à direita
                    tabuleiro[linha_inicial + 1, coluna_inicial] = " " + indicador_adjacente + " ";
                    for (int i = 0; i < posicoes; ++i)
                        tabuleiro[linha_inicial - i, coluna_inicial + 1] = " " + indicador_adjacente + " ";

                }
                // verificar no lado esquerdo
                else if (linha_final > 1 && linha_inicial < linhas_tabuleiro - 1 && coluna_inicial == 1 && coluna_final == 1)
                {
                    // marcar acima, abaixo e N à direita
                    tabuleiro[linha_final - 1, coluna_final] = " " + indicador_adjacente + " ";
                    tabuleiro[linha_inicial + 1, coluna_final] = " " + indicador_adjacente + " ";
                    for (int i = 0; i < posicoes; ++i)
                        tabuleiro[linha_inicial - i, coluna_inicial + 1] = " " + indicador_adjacente + " ";
                }
                // verificar no canto inferior esquerdo
                else if (linha_inicial == linhas_tabuleiro - 1 && coluna_final == 1)
                {
                    // marcar 1 acima e N à direita
                    tabuleiro[linha_final - 1, coluna_inicial] = " " + indicador_adjacente + " ";
                    for (int i = 0; i < posicoes; ++i)
                        tabuleiro[linha_inicial - i, coluna_inicial + 1] = " " + indicador_adjacente + " ";
                }
                // verificar no lado inferior
                else if (linha_inicial == linhas_tabuleiro - 1 && coluna_inicial > 1 && coluna_inicial < colunas_tabuleiro - 1)
                {
                    // marcar 1 acima, N à esquerda e N à direita
                    tabuleiro[linha_final - 1, coluna_inicial] = " " + indicador_adjacente + " ";
                    for (int i = 0; i < posicoes; ++i)
                        tabuleiro[linha_inicial - i, coluna_inicial - 1] = " " + indicador_adjacente + " ";
                    for (int i = 0; i < posicoes; ++i)
                        tabuleiro[linha_inicial - i, coluna_inicial + 1] = " " + indicador_adjacente + " ";

                }
                // verificar no canto inferior direito
                else if (linha_inicial == linhas_tabuleiro - 1 && coluna_inicial == colunas_tabuleiro - 1)
                {
                    // marcar 1 acima e N à esquerda
                    tabuleiro[linha_final - 1, coluna_inicial] = " " + indicador_adjacente + " ";
                    for (int i = 0; i < posicoes; ++i)
                        tabuleiro[linha_inicial - i, coluna_inicial - 1] = " " + indicador_adjacente + " ";
                }
                // verificar no lado direito
                else if (linha_final > 1 && linha_inicial < linhas_tabuleiro - 1 && coluna_inicial == colunas_tabuleiro - 1 && coluna_inicial == coluna_final)
                {
                    // marcar acima, abaixo e N a esquerda
                    tabuleiro[linha_final - 1, coluna_final] = " " + indicador_adjacente + " ";
                    tabuleiro[linha_inicial + 1, coluna_final] = " " + indicador_adjacente + " ";
                    for (int i = 0; i < posicoes; ++i)
                        tabuleiro[linha_inicial - i, coluna_inicial - 1] = " " + indicador_adjacente + " ";
                }
                // verificar no canto superior direito
                else if (linha_final == 1 && coluna_final == colunas_tabuleiro - 1)
                {
                    // marcar abaixo e N a esquerda
                    tabuleiro[linha_inicial + 1, coluna_final] = " " + indicador_adjacente + " ";
                    for (int i = 0; i < posicoes; ++i)
                        tabuleiro[linha_inicial - i, coluna_inicial - 1] = " " + indicador_adjacente + " ";
                }
                // verificar no lado superior
                else if (linha_final == 1 && coluna_final > 1 && coluna_final < colunas_tabuleiro - 1 && coluna_inicial == coluna_final)
                {
                    // marcar N a direita, abaixo e N a esquerda
                    for (int i = 0; i < posicoes; ++i)
                        tabuleiro[linha_inicial - i, coluna_inicial - 1] = " " + indicador_adjacente + " ";
                    tabuleiro[linha_inicial + 1, coluna_inicial] = " " + indicador_adjacente + " ";
                    for (int i = 0; i < posicoes; ++i)
                        tabuleiro[linha_inicial - i, coluna_inicial + 1] = " " + indicador_adjacente + " ";
                }
                // verificar outras posicoes
                else if (linha_final > 1 && linha_inicial < linhas_tabuleiro - 1 && coluna_inicial > 1 && coluna_inicial < colunas_tabuleiro - 1 && coluna_inicial == coluna_final)
                {
                    // marcar N a direita, abaixo, N a esquerda e acima
                    for (int i = 0; i < posicoes; ++i)
                        tabuleiro[linha_inicial - i, coluna_inicial - 1] = " " + indicador_adjacente + " ";
                    tabuleiro[linha_inicial + 1, coluna_inicial] = " " + indicador_adjacente + " ";
                    for (int i = 0; i < posicoes; ++i)
                        tabuleiro[linha_inicial - i, coluna_inicial + 1] = " " + indicador_adjacente + " ";
                    tabuleiro[linha_final - 1, coluna_final] = " " + indicador_adjacente + " ";
                }
            }
            else if (orientacao == "CimaBaixo")
            {
                // verificar no canto superior esquerdo
                if (linha_inicial == 1 && coluna_inicial == 1)
                {
                    // marcar 1 abaixo e N à direita
                    tabuleiro[linha_final + 1, coluna_inicial] = " " + indicador_adjacente + " ";
                    for (int i = 0; i < posicoes; ++i)
                        tabuleiro[linha_inicial + i, coluna_inicial + 1] = " " + indicador_adjacente + " ";

                }
                // verificar no lado esquerdo
                else if (linha_inicial > 1 && linha_final < linhas_tabuleiro - 1 && coluna_inicial == 1 && coluna_final == 1)
                {
                    // marcar acima, abaixo e N à direita
                    tabuleiro[linha_inicial - 1, coluna_final] = " " + indicador_adjacente + " ";
                    tabuleiro[linha_final + 1, coluna_final] = " " + indicador_adjacente + " ";
                    for (int i = 0; i < posicoes; ++i)
                        tabuleiro[linha_inicial + i, coluna_inicial + 1] = " " + indicador_adjacente + " ";
                }
                // verificar no canto inferior esquerdo
                else if (linha_final == linhas_tabuleiro - 1 && coluna_final == 1)
                {
                    // marcar 1 acima e N à direita
                    tabuleiro[linha_inicial - 1, coluna_inicial] = " " + indicador_adjacente + " ";
                    for (int i = 0; i < posicoes; ++i)
                        tabuleiro[linha_inicial + i, coluna_inicial + 1] = " " + indicador_adjacente + " ";
                }
                // verificar no lado inferior
                else if (linha_final == linhas_tabuleiro - 1 && coluna_inicial > 1 && coluna_inicial < colunas_tabuleiro - 1)
                {
                    // marcar 1 acima, N à esquerda e N à direita
                    tabuleiro[linha_inicial - 1, coluna_inicial] = " " + indicador_adjacente + " ";
                    for (int i = 0; i < posicoes; ++i)
                        tabuleiro[linha_inicial + i, coluna_inicial - 1] = " " + indicador_adjacente + " ";
                    for (int i = 0; i < posicoes; ++i)
                        tabuleiro[linha_inicial + i, coluna_inicial + 1] = " " + indicador_adjacente + " ";

                }
                // verificar no canto inferior direito
                else if (linha_final == linhas_tabuleiro - 1 && coluna_final == colunas_tabuleiro - 1)
                {
                    // marcar 1 acima e N à esquerda
                    tabuleiro[linha_inicial - 1, coluna_inicial] = " " + indicador_adjacente + " ";
                    for (int i = 0; i < posicoes; ++i)
                        tabuleiro[linha_inicial + i, coluna_inicial - 1] = " " + indicador_adjacente + " ";
                }
                // verificar no lado direito
                else if (linha_inicial > 1 && linha_final < linhas_tabuleiro - 1 && coluna_inicial == colunas_tabuleiro - 1 && coluna_inicial == coluna_final)
                {
                    // marcar acima, abaixo e N a esquerda
                    tabuleiro[linha_inicial - 1, coluna_final] = " " + indicador_adjacente + " ";
                    tabuleiro[linha_final + 1, coluna_final] = " " + indicador_adjacente + " ";
                    for (int i = 0; i < posicoes; ++i)
                        tabuleiro[linha_inicial + i, coluna_inicial - 1] = " " + indicador_adjacente + " ";
                }
                // verificar no canto superior direito
                else if (linha_inicial == 1 && coluna_inicial == colunas_tabuleiro - 1)
                {
                    // marcar abaixo e N a esquerda
                    tabuleiro[linha_final + 1, coluna_final] = " " + indicador_adjacente + " ";
                    for (int i = 0; i < posicoes; ++i)
                        tabuleiro[linha_inicial + i, coluna_inicial - 1] = " " + indicador_adjacente + " ";
                }
                // verificar no lado superior
                else if (linha_inicial == 1 && coluna_inicial > 1 && coluna_inicial < colunas_tabuleiro - 1 && coluna_inicial == coluna_final)
                {
                    // marcar N a direita, abaixo e N a esquerda
                    for (int i = 0; i < posicoes; ++i)
                        tabuleiro[linha_inicial + i, coluna_inicial - 1] = " " + indicador_adjacente + " ";
                    tabuleiro[linha_final + 1, coluna_inicial] = " " + indicador_adjacente + " ";
                    for (int i = 0; i < posicoes; ++i)
                        tabuleiro[linha_inicial + i, coluna_inicial + 1] = " " + indicador_adjacente + " ";
                }
                // verificar outras posicoes
                else if (linha_inicial > 1 && linha_final < linhas_tabuleiro - 1 && coluna_inicial > 1 && coluna_inicial < colunas_tabuleiro - 1 && coluna_inicial == coluna_final)
                {
                    // marcar N a direita, abaixo, N a esquerda e acima
                    for (int i = 0; i < posicoes; ++i)
                        tabuleiro[linha_inicial + i, coluna_inicial - 1] = " " + indicador_adjacente + " ";
                    tabuleiro[linha_final + 1, coluna_inicial] = " " + indicador_adjacente + " ";
                    for (int i = 0; i < posicoes; ++i)
                        tabuleiro[linha_inicial + i, coluna_inicial + 1] = " " + indicador_adjacente + " ";
                    tabuleiro[linha_inicial - 1, coluna_final] = " " + indicador_adjacente + " ";
                }
            }
            else if (orientacao == "EsqDir")
            {
                // verificar no canto superior esquerdo
                if (linha_inicial == 1 && coluna_inicial == 1)
                {
                    // marcar N abaixo e 1 à direita
                    tabuleiro[linha_inicial, coluna_final + 1] = " " + indicador_adjacente + " ";
                    for (int i = 0; i < posicoes; ++i)
                        tabuleiro[linha_inicial + 1, coluna_inicial + i] = " " + indicador_adjacente + " ";

                }
                // verificar no lado esquerdo
                else if (linha_inicial > 1 && linha_inicial < linhas_tabuleiro - 1 && coluna_inicial == 1 && coluna_final < colunas_tabuleiro - 1)
                {
                    // marcar N acima, N abaixo e 1 à direita
                    for (int i = 0; i < posicoes; ++i)
                        tabuleiro[linha_inicial - 1, coluna_inicial + i] = " " + indicador_adjacente + " ";
                    for (int i = 0; i < posicoes; ++i)
                        tabuleiro[linha_inicial + 1, coluna_inicial + i] = " " + indicador_adjacente + " ";
                    tabuleiro[linha_inicial, coluna_final + 1] = " " + indicador_adjacente + " ";
                }
                // verificar no canto inferior esquerdo
                else if (linha_inicial == linhas_tabuleiro - 1 && coluna_inicial == 1)
                {
                    // marcar N acima e 1 à direita
                    for (int i = 0; i < posicoes; ++i)
                        tabuleiro[linha_inicial - 1, coluna_inicial + i] = " " + indicador_adjacente + " ";
                    tabuleiro[linha_inicial, coluna_final + 1] = " " + indicador_adjacente + " ";
                }
                // verificar no lado inferior
                else if (linha_inicial == linhas_tabuleiro - 1 && coluna_inicial > 1 && coluna_final < colunas_tabuleiro - 1)
                {
                    // marcar N acima, 1 à esquerda e 1 à direita
                    for (int i = 0; i < posicoes; ++i)
                        tabuleiro[linha_inicial - 1, coluna_inicial + i] = " " + indicador_adjacente + " ";
                    tabuleiro[linha_inicial, coluna_inicial - 1] = " " + indicador_adjacente + " ";
                    tabuleiro[linha_inicial, coluna_final + 1] = " " + indicador_adjacente + " ";

                }
                // verificar no canto inferior direito
                else if (linha_inicial == linhas_tabuleiro - 1 && coluna_final == colunas_tabuleiro - 1)
                {
                    // marcar N acima e 1 à esquerda
                    for (int i = 0; i < posicoes; ++i)
                        tabuleiro[linha_inicial - 1, coluna_inicial + i] = " " + indicador_adjacente + " ";
                    tabuleiro[linha_inicial, coluna_inicial - 1] = " " + indicador_adjacente + " ";
                }
                // verificar no lado direito
                else if (linha_inicial > 1 && linha_inicial < linhas_tabuleiro - 1 && coluna_final == colunas_tabuleiro - 1 && coluna_inicial > 1)
                {
                    // marcar N acima, N abaixo e 1 a esquerda
                    for (int i = 0; i < posicoes; ++i)
                        tabuleiro[linha_inicial - 1, coluna_inicial + i] = " " + indicador_adjacente + " ";
                    for (int i = 0; i < posicoes; ++i)
                        tabuleiro[linha_final + 1, coluna_inicial + i] = " " + indicador_adjacente + " ";
                    tabuleiro[linha_inicial, coluna_inicial - 1] = " " + indicador_adjacente + " ";
                }
                // verificar no canto superior direito
                else if (linha_inicial == 1 && coluna_final == colunas_tabuleiro - 1)
                {
                    // marcar N abaixo e 1 a esquerda
                    for (int i = 0; i < posicoes; ++i)
                        tabuleiro[linha_inicial + 1, coluna_inicial + i] = " " + indicador_adjacente + " ";
                    tabuleiro[linha_inicial, coluna_inicial - 1] = " " + indicador_adjacente + " ";
                }
                // verificar no lado superior
                else if (linha_inicial == 1 && coluna_inicial > 1 && coluna_final < colunas_tabuleiro - 1)
                {
                    // marcar 1 a direita, N abaixo e 1 a esquerda
                    tabuleiro[linha_inicial, coluna_final + 1] = " " + indicador_adjacente + " ";
                    for (int i = 0; i < posicoes; ++i)
                        tabuleiro[linha_inicial + 1, coluna_inicial + i] = " " + indicador_adjacente + " ";
                    tabuleiro[linha_inicial, coluna_inicial - 1] = " " + indicador_adjacente + " ";
                }
                // verificar outras posicoes
                else if (linha_inicial > 1 && linha_inicial < linhas_tabuleiro - 1 && coluna_inicial > 1 && coluna_final < colunas_tabuleiro - 1)
                {
                    // marcar 1 a direita, N abaixo, 1 a esquerda e N acima
                    tabuleiro[linha_final, coluna_final + 1] = " " + indicador_adjacente + " ";
                    for (int i = 0; i < posicoes; ++i)
                        tabuleiro[linha_inicial + 1, coluna_inicial + i] = " " + indicador_adjacente + " ";
                    tabuleiro[linha_inicial, coluna_inicial - 1] = " " + indicador_adjacente + " ";
                    for (int i = 0; i < posicoes; ++i)
                        tabuleiro[linha_inicial - 1, coluna_inicial + i] = " " + indicador_adjacente + " ";
                }
            }
            else if (orientacao == "DirEsq")
            {
                // verificar no canto superior esquerdo
                if (linha_final == 1 && coluna_final == 1)
                {
                    // marcar N abaixo e 1 à direita
                    tabuleiro[linha_inicial, coluna_inicial + 1] = " " + indicador_adjacente + " ";
                    for (int i = 0; i < posicoes; ++i)
                        tabuleiro[linha_inicial + 1, coluna_final + i] = " " + indicador_adjacente + " ";
                }
                // verificar no lado esquerdo
                else if (linha_inicial > 1 && linha_inicial < linhas_tabuleiro - 1 && coluna_final == 1 && coluna_inicial < colunas_tabuleiro - 1)
                {
                    // marcar N acima, N abaixo e 1 à direita
                    for (int i = 0; i < posicoes; ++i)
                        tabuleiro[linha_inicial - 1, coluna_final + i] = " " + indicador_adjacente + " ";
                    for (int i = 0; i < posicoes; ++i)
                        tabuleiro[linha_inicial + 1, coluna_final + i] = " " + indicador_adjacente + " ";
                    tabuleiro[linha_inicial, coluna_inicial + 1] = " " + indicador_adjacente + " ";
                }
                // verificar no canto inferior esquerdo
                else if (linha_inicial == linhas_tabuleiro - 1 && coluna_final == 1)
                {
                    // marcar N acima e 1 à direita
                    for (int i = 0; i < posicoes; ++i)
                        tabuleiro[linha_inicial - 1, coluna_final + i] = " " + indicador_adjacente + " ";
                    tabuleiro[linha_inicial, coluna_inicial + 1] = " " + indicador_adjacente + " ";
                }
                // verificar no lado inferior
                else if (linha_inicial == linhas_tabuleiro - 1 && coluna_final > 1 && coluna_inicial < colunas_tabuleiro - 1)
                {
                    // marcar N acima, 1 à esquerda e 1 à direita
                    for (int i = 0; i < posicoes; ++i)
                        tabuleiro[linha_inicial - 1, coluna_final + i] = " " + indicador_adjacente + " ";
                    tabuleiro[linha_inicial, coluna_final - 1] = " " + indicador_adjacente + " ";
                    tabuleiro[linha_inicial, coluna_inicial + 1] = " " + indicador_adjacente + " ";

                }
                // verificar no canto inferior direito
                else if (linha_inicial == linhas_tabuleiro - 1 && coluna_inicial == colunas_tabuleiro - 1)
                {
                    // marcar N acima e 1 à esquerda
                    for (int i = 0; i < posicoes; ++i)
                        tabuleiro[linha_inicial - 1, coluna_final + i] = " " + indicador_adjacente + " ";
                    tabuleiro[linha_inicial, coluna_final - 1] = " " + indicador_adjacente + " ";
                }
                // verificar no lado direito
                else if (linha_inicial > 1 && linha_inicial < linhas_tabuleiro - 1 && coluna_inicial == colunas_tabuleiro - 1 && coluna_final > 1)
                {
                    // marcar N acima, N abaixo e 1 a esquerda
                    for (int i = 0; i < posicoes; ++i)
                        tabuleiro[linha_inicial - 1, coluna_final + i] = " " + indicador_adjacente + " ";
                    for (int i = 0; i < posicoes; ++i)
                        tabuleiro[linha_final + 1, coluna_final + i] = " " + indicador_adjacente + " ";
                    tabuleiro[linha_inicial, coluna_final - 1] = " " + indicador_adjacente + " ";
                }
                // verificar no canto superior direito
                else if (linha_inicial == 1 && coluna_inicial == colunas_tabuleiro - 1)
                {
                    // marcar N abaixo e 1 a esquerda
                    for (int i = 0; i < posicoes; ++i)
                        tabuleiro[linha_inicial + 1, coluna_final + i] = " " + indicador_adjacente + " ";
                    tabuleiro[linha_inicial, coluna_final - 1] = " " + indicador_adjacente + " ";
                }
                // verificar no lado superior
                else if (linha_inicial == 1 && coluna_final > 1 && coluna_inicial < colunas_tabuleiro - 1)
                {
                    // marcar 1 a direita, N abaixo e 1 a esquerda
                    tabuleiro[linha_inicial, coluna_inicial + 1] = " " + indicador_adjacente + " ";
                    for (int i = 0; i < posicoes; ++i)
                        tabuleiro[linha_inicial + 1, coluna_final + i] = " " + indicador_adjacente + " ";
                    tabuleiro[linha_inicial, coluna_final - 1] = " " + indicador_adjacente + " ";
                }
                // verificar outras posicoes
                else if (linha_inicial > 1 && linha_inicial < linhas_tabuleiro - 1 && coluna_final > 1 && coluna_inicial < colunas_tabuleiro - 1)
                {
                    // marcar 1 a direita, N abaixo, 1 a esquerda e N acima
                    tabuleiro[linha_final, coluna_inicial + 1] = " " + indicador_adjacente + " ";
                    for (int i = 0; i < posicoes; ++i)
                        tabuleiro[linha_inicial + 1, coluna_final + i] = " " + indicador_adjacente + " ";
                    tabuleiro[linha_inicial, coluna_final - 1] = " " + indicador_adjacente + " ";
                    for (int i = 0; i < posicoes; ++i)
                        tabuleiro[linha_inicial - 1, coluna_final + i] = " " + indicador_adjacente + " ";
                }
            }
        }

        private static bool VerificarNavio(string[,] tabuleiro, int linha, int coluna, char tipo_navio)
        {
            bool resultado = false;

            // verificar no canto superior esquerdo
            if (linha == 1 && coluna == 1)
            {
                // verificar abaixo e a direita
                if (tabuleiro[linha + 1, coluna].Contains(tipo_navio) || tabuleiro[linha, coluna + 1].Contains(tipo_navio))
                {
                    resultado = true;
                    tabuleiro[linha + 1, coluna] = " " + indicador_adjacente + " ";
                    tabuleiro[linha, coluna + 1] = " " + indicador_adjacente + " ";
                }

            }
            // verificar no lado esquerdo
            else if (linha > 1 && linha < linhas_tabuleiro - 1 && coluna == 1)
            {
                // verificar acima, abaixo e a direita
                if (tabuleiro[linha - 1, coluna].Contains(tipo_navio) || tabuleiro[linha + 1, coluna].Contains(tipo_navio) || tabuleiro[linha, coluna + 1].Contains(tipo_navio))
                {
                    resultado = true;
                    tabuleiro[linha - 1, coluna] = " " + indicador_adjacente + " ";
                    tabuleiro[linha + 1, coluna] = " " + indicador_adjacente + " ";
                    tabuleiro[linha, coluna + 1] = " " + indicador_adjacente + " ";
                }
            }
            // verificar no canto inferior esquerdo
            else if (linha == linhas_tabuleiro - 1 && coluna == 1)
            {
                // verificar acima e a direita
                if (tabuleiro[linha - 1, coluna].Contains(tipo_navio) || tabuleiro[linha, coluna + 1].Contains(tipo_navio))
                {
                    resultado = true;
                    tabuleiro[linha - 1, coluna] = " " + indicador_adjacente + " ";
                    tabuleiro[linha, coluna + 1] = " " + indicador_adjacente + " ";
                }
            }
            // verificar no lado inferior
            else if (linha == linhas_tabuleiro - 1 && coluna > 1 && coluna < colunas_tabuleiro - 1)
            {
                // verificar acima, esquerda e a direita
                if (tabuleiro[linha - 1, coluna].Contains(tipo_navio) || tabuleiro[linha, coluna - 1].Contains(tipo_navio) || tabuleiro[linha, coluna + 1].Contains(tipo_navio))
                {
                    resultado = true;
                    tabuleiro[linha - 1, coluna] = " " + indicador_adjacente + " ";
                    tabuleiro[linha, coluna - 1] = " " + indicador_adjacente + " ";
                    tabuleiro[linha, coluna + 1] = " " + indicador_adjacente + " ";
                }
            }
            // verificar no canto inferior direito
            else if (linha == linhas_tabuleiro - 1 && coluna == colunas_tabuleiro - 1)
            {
                // verificar acima e a esquerda
                if (tabuleiro[linha - 1, coluna].Contains(tipo_navio) || tabuleiro[linha, coluna - 1].Contains(tipo_navio))
                {
                    resultado = true;
                    tabuleiro[linha - 1, coluna] = " " + indicador_adjacente + " ";
                    tabuleiro[linha, coluna - 1] = " " + indicador_adjacente + " ";
                }
            }
            // verificar no lado direito
            else if (linha > 1 && linha < linhas_tabuleiro - 1 && coluna == colunas_tabuleiro - 1)
            {
                // verificar acima, abaixo e a esquerda
                if (tabuleiro[linha - 1, coluna].Contains(tipo_navio) || tabuleiro[linha + 1, coluna].Contains(tipo_navio) || tabuleiro[linha, coluna - 1].Contains(tipo_navio))
                {
                    resultado = true;
                    tabuleiro[linha - 1, coluna] = " " + indicador_adjacente + " ";
                    tabuleiro[linha + 1, coluna] = " " + indicador_adjacente + " ";
                    tabuleiro[linha, coluna - 1] = " " + indicador_adjacente + " ";
                }
            }
            // verificar no canto superior direito
            else if (linha == 1 && coluna == colunas_tabuleiro - 1)
            {
                // verificar abaixo e a esquerda
                if (tabuleiro[linha + 1, coluna].Contains(tipo_navio) || tabuleiro[linha, coluna - 1].Contains(tipo_navio))
                {
                    resultado = true;
                    tabuleiro[linha + 1, coluna] = " " + indicador_adjacente + " ";
                    tabuleiro[linha, coluna - 1] = " " + indicador_adjacente + " ";
                }
            }
            // verificar no lado superior
            else if (linha == 1 && coluna > 1 && coluna < colunas_tabuleiro - 1)
            {
                // verificar direita, abaixo e a esquerda
                if (tabuleiro[linha, coluna + 1].Contains(tipo_navio) || tabuleiro[linha + 1, coluna].Contains(tipo_navio) || tabuleiro[linha, coluna - 1].Contains(tipo_navio))
                {
                    resultado = true;
                    tabuleiro[linha, coluna + 1] = " " + indicador_adjacente + " ";
                    tabuleiro[linha + 1, coluna] = " " + indicador_adjacente + " ";
                    tabuleiro[linha, coluna - 1] = " " + indicador_adjacente + " ";
                }
            }
            // verificar outras posicoes
            else if (linha > 1 && linha < linhas_tabuleiro - 1 && coluna > 1 && coluna < colunas_tabuleiro - 1)
            {
                // verificar direita, abaixo, esquerda e acima
                if (tabuleiro[linha, coluna - 1].Contains(tipo_navio) || tabuleiro[linha + 1, coluna].Contains(tipo_navio) || tabuleiro[linha, coluna + 1].Contains(tipo_navio) || tabuleiro[linha - 1, coluna].Contains(tipo_navio))
                {
                    resultado = true;
                    tabuleiro[linha, coluna - 1] = " " + indicador_adjacente + " ";
                    tabuleiro[linha + 1, coluna] = " " + indicador_adjacente + " ";
                    tabuleiro[linha, coluna + 1] = " " + indicador_adjacente + " ";
                    tabuleiro[linha - 1, coluna] = " " + indicador_adjacente + " ";
                }
            }
            return resultado;
        }

        private static bool VerificarJogadaPc(string[,] tabuleiro, int linha, int coluna)
        {
            // TODO: melhorar IA

            bool resultado = false;

            // caso simples, a posicao não está preenchida
            if (tabuleiro[linha, coluna].Contains(indicador_agua))
            // nao dar tiro em posicoes adjacentes aos Submarinos
            // verificar se há barcos nas posicoes adjacentes
            {
                // verificar no canto superior esquerdo
                if (linha == 1 && coluna == 1)
                {
                    // verificar abaixo e a direita
                    if (!tabuleiro[linha + 1, coluna].Contains(indicador_submarino) && !tabuleiro[linha, coluna + 1].Contains(indicador_submarino))
                    {
                        resultado = true;
                    }

                }
                // verificar no lado esquerdo
                else if (linha > 1 && linha < linhas_tabuleiro - 1 && coluna == 1)
                {
                    // verificar acima, abaixo e a direita
                    if (!tabuleiro[linha - 1, coluna].Contains(indicador_submarino) && !tabuleiro[linha + 1, coluna].Contains(indicador_submarino) && !tabuleiro[linha, coluna + 1].Contains(indicador_submarino))
                    {
                        resultado = true;
                    }
                }
                // verificar no canto inferior esquerdo
                else if (linha == linhas_tabuleiro - 1 && coluna == 1)
                {
                    // verificar acima e a direita
                    if (!tabuleiro[linha - 1, coluna].Contains(indicador_submarino) && !tabuleiro[linha, coluna + 1].Contains(indicador_submarino))
                    {
                        resultado = true;
                    }
                }
                // verificar no lado inferior
                else if (linha == linhas_tabuleiro - 1 && coluna > 1 && coluna < colunas_tabuleiro - 1)
                {
                    // verificar acima, esquerda e a direita
                    if (!tabuleiro[linha - 1, coluna].Contains(indicador_submarino) && !tabuleiro[linha, coluna - 1].Contains(indicador_submarino) && !tabuleiro[linha, coluna + 1].Contains(indicador_submarino))
                    {
                        resultado = true;
                    }
                }
                // verificar no canto inferior direito
                else if (linha == linhas_tabuleiro - 1 && coluna == colunas_tabuleiro - 1)
                {
                    // verificar acima e a esquerda
                    if (!tabuleiro[linha - 1, coluna].Contains(indicador_submarino) && !tabuleiro[linha, coluna - 1].Contains(indicador_submarino))
                    {
                        resultado = true;
                    }
                }
                // verificar no lado direito
                else if (linha > 1 && linha < linhas_tabuleiro - 1 && coluna == colunas_tabuleiro - 1)
                {
                    // verificar acima, abaixo e a esquerda
                    if (!tabuleiro[linha - 1, coluna].Contains(indicador_submarino) && !tabuleiro[linha + 1, coluna].Contains(indicador_submarino) && !tabuleiro[linha, coluna - 1].Contains(indicador_submarino))
                    {
                        resultado = true;
                    }
                }
                // verificar no canto superior direito
                else if (linha == 1 && coluna == colunas_tabuleiro - 1)
                {
                    // verificar abaixo e a esquerda
                    if (!tabuleiro[linha + 1, coluna].Contains(indicador_submarino) && !tabuleiro[linha, coluna - 1].Contains(indicador_submarino))
                    {
                        resultado = true;
                    }
                }
                // verificar no lado superior
                else if (linha == 1 && coluna > 1 && coluna < colunas_tabuleiro - 1)
                {
                    // verificar direita, abaixo e a esquerda
                    if (!tabuleiro[linha, coluna + 1].Contains(indicador_submarino) && !tabuleiro[linha + 1, coluna].Contains(indicador_submarino) && !tabuleiro[linha, coluna - 1].Contains(indicador_submarino))
                    {
                        resultado = true;
                    }
                }
                // verificar outras posicoes
                else if (linha > 1 && linha < linhas_tabuleiro - 1 && coluna > 1 && coluna < colunas_tabuleiro - 1)
                {
                    // verificar direita, abaixo, esquerda e acima
                    if (!tabuleiro[linha, coluna - 1].Contains(indicador_submarino) && !tabuleiro[linha + 1, coluna].Contains(indicador_submarino) && !tabuleiro[linha, coluna + 1].Contains(indicador_submarino) && !tabuleiro[linha - 1, coluna].Contains(indicador_submarino))
                    {
                        resultado = true;
                    }
                }
            }
            return resultado;
        }

        private static void ImprimirTabuleiro(string[,] tabuleiro)
        {
            for (int i = 0; i < linhas_tabuleiro; ++i)
            {
                for (int j = 0; j < colunas_tabuleiro; ++j)
                {
                    Console.Write("|");
                    if (tabuleiro[i, j].Contains(indicador_agua))
                        Console.ForegroundColor = ConsoleColor.Blue;
                    else if ((tabuleiro[i, j].Contains(indicador_porta_avioes) && i > 0) || (tabuleiro[i, j].Contains(indicador_battleship) && i > 0) || (tabuleiro[i, j].Contains(indicador_destroyer) && i > 0) || tabuleiro[i, j].Contains(indicador_submarino))
                        Console.ForegroundColor = ConsoleColor.Green;
                    else if (tabuleiro[i, j].Contains(indicador_tiro_na_agua))
                        Console.ForegroundColor = ConsoleColor.Red;
                    else
                        Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write(tabuleiro[i, j]);
                    Console.ResetColor();
                }
                Console.Write("|\n");
                for (int k = 0; k < colunas_tabuleiro; ++k)
                {
                    Console.Write("----");
                }
                Console.WriteLine();
            }
        }

        private static void InicializarJogoPc()
        {
            // alocar espaco para a linha de letras e a coluna de numeros
            colunas_tabuleiro++;
            linhas_tabuleiro++;

            // tabuleiros onde os jogadores colocam os navios
            string[,] tabuleiro_jogador1 = new string[linhas_tabuleiro, colunas_tabuleiro];
            string[,] tabuleiro_pc = new string[linhas_tabuleiro, colunas_tabuleiro];

            // tabuleiros de jogo onde os jogadores tentam afundar os navios do adversario
            string[,] tabuleiro_de_jogo_jogador1 = new string[linhas_tabuleiro, colunas_tabuleiro];
            string[,] tabuleiro_de_jogo_pc = new string[linhas_tabuleiro, colunas_tabuleiro];

            InicializarTabuleiros(tabuleiro_jogador1, tabuleiro_pc, tabuleiro_de_jogo_jogador1, tabuleiro_de_jogo_pc);

            do
            {
                Console.Clear();
                Console.WriteLine("Jogador 1 - Insere o teu nome:");
                nome_jogador1 = Console.ReadLine();
            } while (nome_jogador1 == "");
            InicializarJogador(tabuleiro_jogador1, nome_jogador1);

            InicializarPc(tabuleiro_pc);

            Console.Clear();
            JogarContraPc(tabuleiro_jogador1, tabuleiro_pc, tabuleiro_de_jogo_jogador1, tabuleiro_de_jogo_pc);

            // reverter tamanho de tabuleiro para o valor definido
            colunas_tabuleiro--;
            linhas_tabuleiro--;
        }

        private static void InicializarPc(string[,] tabuleiro)
        {
            if (linhas_tabuleiro > 7 && colunas_tabuleiro > 7)
            {
                Console.Clear();
                // inicializar porta avioes
                PreencherPc(4, tabuleiro);
                Console.WriteLine();
                // DEBUG:
                if (mostrar_tabuleiro_pc == "true")
                    ImprimirTabuleiro(tabuleiro);
                Thread.Sleep(1000);
            }

            Console.Clear();
            // inicializar Battle Ship
            PreencherPc(3, tabuleiro);
            Console.WriteLine();
            // DEBUG:
            if (mostrar_tabuleiro_pc == "true")
                ImprimirTabuleiro(tabuleiro);
            Thread.Sleep(1000);

            // Colocar um destroyer extra
            if (linhas_tabuleiro > 7 && colunas_tabuleiro > 11)
            {
                Console.Clear();
                PreencherPc(2, tabuleiro);
                Console.WriteLine();
                // DEBUG:
                if (mostrar_tabuleiro_pc == "true")
                    ImprimirTabuleiro(tabuleiro);
                Thread.Sleep(1000);
            }

            Console.Clear();
            // inicializar Destroyer
            PreencherPc(2, tabuleiro);
            Console.WriteLine();
            // DEBUG:
            if (mostrar_tabuleiro_pc == "true")
                ImprimirTabuleiro(tabuleiro);
            Thread.Sleep(1000);

            // Colocar um submarino extra
            if (linhas_tabuleiro > 7 && colunas_tabuleiro > 7)
            {
                Console.Clear();
                // inicializar Submarino
                PreencherPc(1, tabuleiro);
                Console.WriteLine();
                // DEBUG:
                if (mostrar_tabuleiro_pc == "true")
                    ImprimirTabuleiro(tabuleiro);
                Thread.Sleep(1000);
            }

            Console.Clear();
            // inicializar Submarino
            PreencherPc(1, tabuleiro);
            Console.WriteLine();
            // DEBUG:
            if (mostrar_tabuleiro_pc == "true")
                ImprimirTabuleiro(tabuleiro);
            Thread.Sleep(1000);
        }

        private static void PreencherPc(int posicoes, string[,] tabuleiro)
        {
            Random rnd = new Random();
            int linha_inicial = 0;
            int coluna_inicial = 0;
            int linha_final = 0;
            int coluna_final = 0;
            string nome_navio = "";

            switch (posicoes)
            {
                case 1:
                    nome_navio = "Submarino";
                    break;
                case 2:
                    nome_navio = "Destroyer";
                    break;
                case 3:
                    nome_navio = "Battleship";
                    break;
                case 4:
                    nome_navio = "Porta Avioes";
                    break;
                default:
                    nome_navio = "Inesperado";
                    break;
            }

            Console.WriteLine("O PC está a colocar o " + nome_navio + "...");

            if (posicoes > 1)
            {
                do
                {
                    int pos = 0;
                    linha_inicial = rnd.Next(1, linhas_tabuleiro);
                    coluna_inicial = rnd.Next(1, colunas_tabuleiro);
                    if ((linha_inicial + posicoes - 1) < linhas_tabuleiro && (pos = rnd.Next(1, 3)) == 1)
                    {
                        linha_final = linha_inicial + posicoes - 1;
                        coluna_final = coluna_inicial;
                    }
                    else if ((coluna_inicial + posicoes - 1) < colunas_tabuleiro && (pos = rnd.Next(1, 3)) == 1)
                    {
                        linha_final = linha_inicial;
                        coluna_final = coluna_inicial + posicoes - 1;
                    }
                    else if ((linha_inicial - posicoes + 1) > 0 && (pos = rnd.Next(1, 3)) == 1)
                    {
                        linha_final = linha_inicial - posicoes + 1;
                        coluna_final = coluna_inicial;
                    }
                    else
                    {
                        linha_final = rnd.Next(1, linhas_tabuleiro);
                        coluna_final = rnd.Next(1, colunas_tabuleiro);
                    }
                } while (!VerificarPosicao(tabuleiro, linha_inicial, coluna_inicial, linha_final, coluna_final, posicoes, "PC"));
                MarcarPosicoes(tabuleiro, linha_inicial, coluna_inicial, linha_final, coluna_final, posicoes);
            }
            else
            {
                do
                {
                    linha_inicial = rnd.Next(1, linhas_tabuleiro);
                    coluna_inicial = rnd.Next(1, colunas_tabuleiro);
                    linha_final = linha_inicial;
                    coluna_final = coluna_inicial;
                } while (!VerificarPosicao(tabuleiro, linha_inicial, coluna_inicial, linha_final, coluna_final, posicoes, "PC"));
                // ! TESTAR !
                MarcarPosicoes(tabuleiro, linha_inicial, coluna_inicial, linha_final, coluna_final, posicoes);
            }
        }

        private static void JogarContraPc(string[,] tabuleiro_jogador1, string[,] tabuleiro_pc, string[,] tabuleiro_de_jogo_jogador1, string[,] tabuleiro_de_jogo_pc)
        {
            int tentativas_jogador1 = 0;
            int tentativas_pc = 0;

            int barcos_afundados_jogador1 = 0;
            int barcos_afundados_pc = 0;

            string calcular_vez = "A calcular vez";

            Random rnd = new Random();
            int vez = rnd.Next(1, 3);

            bool acertou = false;
            bool fim_de_jogo = false;

            for (int i = 0; i < 4; ++i)
            {
                Console.WriteLine(calcular_vez);
                calcular_vez += ".";
                Thread.Sleep(500);
                Console.Clear();
            }

            while (!fim_de_jogo)
            {
                if (vez == 1)
                {
                    do
                    {
                        acertou = JogadaJogador(nome_jogador1, tabuleiro_de_jogo_jogador1, tabuleiro_pc, ref barcos_afundados_jogador1, ref tentativas_jogador1);

                        if (barcos_afundados_jogador1 == numero_posicoes_navios)
                        {
                            FimDeJogo(tabuleiro_de_jogo_jogador1, tentativas_jogador1, nome_jogador1);
                            fim_de_jogo = true;
                            break;
                        }
                        else
                        {
                            if (acertou)
                                Console.WriteLine("Prime uma tecla para continuar...");
                            else
                            {
                                vez = 2;
                                Console.WriteLine("Prime uma tecla para dar a vez...");
                            }
                            Console.ReadKey();
                        }
                    } while (acertou);
                }
                else
                {
                    do
                    {
                        acertou = JogadaPc(tabuleiro_de_jogo_pc, tabuleiro_jogador1, ref barcos_afundados_pc, ref tentativas_pc, barcos_afundados_jogador1, tentativas_jogador1);

                        if (barcos_afundados_pc == numero_posicoes_navios)
                        {
                            Console.WriteLine();
                            Console.WriteLine("O PC ganhou!");
                            Console.WriteLine();
                            FimDeJogoPC(tabuleiro_de_jogo_jogador1, tentativas_jogador1, nome_jogador1);
                            fim_de_jogo = true;
                            break;
                        }
                        else
                        {
                            if (acertou)
                                Thread.Sleep(2000);
                            else
                            {
                                vez = 1;
                                Thread.Sleep(2000);
                            }
                        }
                    } while (acertou);
                }
            }
        }

        private static bool JogadaPc(string[,] tabuleiro_de_jogo_pc, string[,] tabuleiro_jogador1, ref int barcos_afundados_pc, ref int tentativas_pc, int barcos_afundados_jogador1, int tentativas_jogador1)
        {
            Random rnd = new Random();

            int linha = 0;
            int coluna = 0;

            bool acertou = false;

            string pc_a_jogar = "O PC está a jogar";

            for (int i = 0; i < 4; ++i)
            {
                Console.Clear();
                Console.WriteLine(pc_a_jogar);
                Console.WriteLine();
                ImprimirTabuleiro(tabuleiro_de_jogo_pc);
                Console.WriteLine();
                Console.WriteLine("Tentativas: " + tentativas_pc);
                Console.WriteLine("Alvos atingidos: " + barcos_afundados_pc);
                Console.WriteLine();
                pc_a_jogar += ".";
                Thread.Sleep(600);
            }

            // TODO: IA

            do
            {
                linha = rnd.Next(1, linhas_tabuleiro);
                coluna = rnd.Next(1, colunas_tabuleiro);

                if (tentativas_jogador1 - tentativas_pc > 2 && barcos_afundados_jogador1 >= barcos_afundados_pc)
                {
                    do
                    {
                        linha = rnd.Next(1, linhas_tabuleiro);
                        coluna = rnd.Next(1, colunas_tabuleiro);
                    } while (!PcGanhaJogada(tabuleiro_jogador1, linha, coluna));
                }

            } while (!VerificarJogadaPc(tabuleiro_de_jogo_pc, linha, coluna));

            tentativas_pc++;

            Console.Clear();
            if (tabuleiro_jogador1[linha, coluna].Contains(indicador_porta_avioes))
            {
                tabuleiro_de_jogo_pc[linha, coluna] = " " + indicador_porta_avioes + " ";
                acertou = true;
            }
            else if (tabuleiro_jogador1[linha, coluna].Contains(indicador_battleship))
            {
                tabuleiro_de_jogo_pc[linha, coluna] = " " + indicador_battleship + " ";
                acertou = true;
            }
            else if (tabuleiro_jogador1[linha, coluna].Contains(indicador_destroyer))
            {
                tabuleiro_de_jogo_pc[linha, coluna] = " " + indicador_destroyer + " ";
                acertou = true;
            }
            else if (tabuleiro_jogador1[linha, coluna].Contains(indicador_submarino))
            {
                tabuleiro_de_jogo_pc[linha, coluna] = " " + indicador_submarino + " ";
                acertou = true;
            }
            else
            {
                tabuleiro_de_jogo_pc[linha, coluna] = " " + indicador_tiro_na_agua + " ";
                acertou = false;
            }

            if (acertou)
            {
                SoundPlayer player = new SoundPlayer(Properties.Resources.Depth_Charge);
                player.Play();
                barcos_afundados_pc++;
                Console.WriteLine("O PC acertou!");
            }
            else
            {
                Console.WriteLine("O PC errou!");
            }

            Console.WriteLine();
            ImprimirTabuleiro(tabuleiro_de_jogo_pc);
            Console.WriteLine();
            Console.WriteLine("Tentativas: " + tentativas_pc);
            Console.WriteLine("Alvos atingidos: " + barcos_afundados_pc);
            Console.WriteLine();

            return acertou;
        }

        private static bool PcGanhaJogada(string[,] tabuleiro_oponente, int linha, int coluna)
        {
            bool resultado = false;

            if (!tabuleiro_oponente[linha, coluna].Contains(indicador_agua) && !tabuleiro_oponente[linha, coluna].Contains(indicador_adjacente))
                resultado = true;

            return resultado;
        }
    }
}
