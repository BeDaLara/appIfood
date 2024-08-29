using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace appIfood
{
    class Program
    {
       private static string connectionString = "Server=localhost;Database=db_livros;Uid=root;Pwd=1234567;SslMode=none;";

        static void Main(string[] args)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("1. Cadastrar Restaurante");
                Console.WriteLine("2. Cadastrar Prato");
                Console.WriteLine("3. Cadastrar Cliente");
                Console.WriteLine("4. Realizar Pedido");
                Console.WriteLine("5. Listar Pedidos");
                Console.WriteLine("6. Sair");
                Console.Write("Escolha uma opção: ");
                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        CadastrarRestaurante();
                        break;
                    case "2":
                        CadastrarPrato();
                        break;
                    case "3":
                        CadastrarCliente();
                        break;
                    case "4":
                        RealizarPedido();
                        break;
                    case "5":
                        ListarPedidos();
                        break;
                    case "6":
                        return;
                    default:
                        Console.WriteLine("Opção inválida.");
                        break;
                }
            }
        }

        static void CadastrarRestaurante()
        {
            using var connection = new MySqlConnection(connectionString);
            connection.Open();

            Console.Write("Nome do Restaurante: ");
            string nome = Console.ReadLine();
            Console.Write("Endereço: ");
            string endereco = Console.ReadLine();
            Console.Write("Telefone: ");
            string telefone = Console.ReadLine();

            string query = "INSERT INTO restaurantes (Nome, Endereco, Telefone) VALUES (@Nome, @Endereco, @Telefone)";
            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@Nome", nome);
            cmd.Parameters.AddWithValue("@Endereco", endereco);
            cmd.Parameters.AddWithValue("@Telefone", telefone);

            cmd.ExecuteNonQuery();
            Console.WriteLine("Restaurante cadastrado com sucesso!");
        }

        static void CadastrarPrato()
        {
            using var connection = new MySqlConnection(connectionString);
            connection.Open();

            Console.Write("Nome do Prato: ");
            string nome = Console.ReadLine();
            Console.Write("Descrição: ");
            string descricao = Console.ReadLine();
            Console.Write("Preço: ");
            decimal preco = Convert.ToDecimal(Console.ReadLine());
            Console.Write("ID do Restaurante: ");
            int restauranteId = Convert.ToInt32(Console.ReadLine());

            string queryCheck = "SELECT COUNT(*) FROM pratos WHERE Nome = @Nome AND RestauranteId = @RestauranteId";
            using var cmdCheck = new MySqlCommand(queryCheck, connection);
            cmdCheck.Parameters.AddWithValue("@Nome", nome);
            cmdCheck.Parameters.AddWithValue("@RestauranteId", restauranteId);

            var count = Convert.ToInt32(cmdCheck.ExecuteScalar());
            if (count > 0)
            {
                Console.WriteLine("Prato já existe no restaurante.");
                return;
            }

            string query = "INSERT INTO pratos (RestauranteId, Nome, Descricao, Preco) VALUES (@RestauranteId, @Nome, @Descricao, @Preco)";
            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@RestauranteId", restauranteId);
            cmd.Parameters.AddWithValue("@Nome", nome);
            cmd.Parameters.AddWithValue("@Descricao", descricao);
            cmd.Parameters.AddWithValue("@Preco", preco);

            cmd.ExecuteNonQuery();
            Console.WriteLine("Prato cadastrado com sucesso!");
        }

        static void CadastrarCliente()
        {
            using var connection = new MySqlConnection(connectionString);
            connection.Open();

            Console.Write("Nome do Cliente: ");
            string nome = Console.ReadLine();
            Console.Write("Endereço: ");
            string endereco = Console.ReadLine();
            Console.Write("Telefone: ");
            string telefone = Console.ReadLine();

            string query = "INSERT INTO clientes (Nome, Endereco, Telefone) VALUES (@Nome, @Endereco, @Telefone)";
            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@Nome", nome);
            cmd.Parameters.AddWithValue("@Endereco", endereco);
            cmd.Parameters.AddWithValue("@Telefone", telefone);

            cmd.ExecuteNonQuery();
            Console.WriteLine("Cliente cadastrado com sucesso!");
        }

        static void RealizarPedido()
        {
            using var connection = new MySqlConnection(connectionString);
            connection.Open();

            Console.Write("ID do Cliente: ");
            int clienteId = Convert.ToInt32(Console.ReadLine());
            Console.Write("ID do Restaurante: ");
            int restauranteId = Convert.ToInt32(Console.ReadLine());
            Console.Write("Data do Pedido (YYYY-MM-DD HH:MM:SS): ");
            var dataPedido = DateTime.Parse(Console.ReadLine());
            Console.Write("Status do Pedido: ");
            string statusPedido = Console.ReadLine();

            decimal total = 0;
            var itens = new List<(int pratoId, int quantidade, decimal preco)>();
            while (true)
            {
                Console.Write("ID do Prato (0 para finalizar): ");
                int pratoId = Convert.ToInt32(Console.ReadLine());
                if (pratoId == 0)
                {
                    break;
                }
                Console.Write("Quantidade: ");
                int quantidade = Convert.ToInt32(Console.ReadLine());

                string queryPreco = "SELECT Preco FROM pratos WHERE Id = @PratoId";
                using var cmdPreco = new MySqlCommand(queryPreco, connection);
                cmdPreco.Parameters.AddWithValue("@PratoId", pratoId);
                decimal preco = Convert.ToDecimal(cmdPreco.ExecuteScalar());

                total += preco * quantidade;
                itens.Add((pratoId, quantidade, preco));
            }

            string queryPedido = "INSERT INTO pedidos (ClienteId, RestauranteId, DataPedido, StatusPedido, Total) VALUES (@ClienteId, @RestauranteId, @DataPedido, @StatusPedido, @Total)";
            using var cmdPedido = new MySqlCommand(queryPedido, connection);
            cmdPedido.Parameters.AddWithValue("@ClienteId", clienteId);
            cmdPedido.Parameters.AddWithValue("@RestauranteId", restauranteId);
            cmdPedido.Parameters.AddWithValue("@DataPedido", dataPedido);
            cmdPedido.Parameters.AddWithValue("@StatusPedido", statusPedido);
            cmdPedido.Parameters.AddWithValue("@Total", total);

            cmdPedido.ExecuteNonQuery();
            var pedidoId = (int)cmdPedido.LastInsertedId;

            string queryItem = "INSERT INTO itens_pedido (PedidoId, PratoId, Quantidade, Preco) VALUES (@PedidoId, @PratoId, @Quantidade, @Preco)";
            foreach (var item in itens)
            {
                using var cmdItem = new MySqlCommand(queryItem, connection);
                cmdItem.Parameters.AddWithValue("@PedidoId", pedidoId);
                cmdItem.Parameters.AddWithValue("@PratoId", item.pratoId);
                cmdItem.Parameters.AddWithValue("@Quantidade", item.quantidade);
                cmdItem.Parameters.AddWithValue("@Preco", item.preco);

                cmdItem.ExecuteNonQuery();
            }

            Console.WriteLine("Pedido realizado com sucesso!");
        }

        static void ListarPedidos()
        {
            using var connection = new MySqlConnection(connectionString);
            connection.Open();

            var query = @"
                SELECT p.Id AS PedidoId, c.Nome AS Cliente, r.Nome AS Restaurante, p.DataPedido, p.StatusPedido, p.Total
                FROM pedidos p
                INNER JOIN clientes c ON p.ClienteId = c.Id
                INNER JOIN restaurantes r ON p.RestauranteId = r.Id";

            using var cmd = new MySqlCommand(query, connection);
            using var reader = cmd.ExecuteReader();

            Console.WriteLine("Pedidos:");
            while (reader.Read())
            {
                Console.WriteLine($"ID do Pedido: {reader["PedidoId"]}");
                Console.WriteLine($"Cliente: {reader["Cliente"]}");
                Console.WriteLine($"Restaurante: {reader["Restaurante"]}");
                Console.WriteLine($"Data do Pedido: {reader["DataPedido"]}");
                Console.WriteLine($"Status do Pedido: {reader["StatusPedido"]}");
                Console.WriteLine($"Total: {reader["Total"]}");
                Console.WriteLine("------------------------------");
            }
        }
    }
}
