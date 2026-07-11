using System.Reflection;
using DbUp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("yuki-blog-database");

if (string.IsNullOrEmpty(connectionString))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Erro: Connection string 'yuki-blog-database' não foi fornecida pelo Aspire.");
    Console.ResetColor();
    return -1; // Código de erro para o Aspire segurar a API
}

Console.WriteLine("Iniciando as migrações do banco de dados...");

EnsureDatabase.For.PostgresqlDatabase(connectionString);

var upgrader = DeployChanges.To
    .PostgresqlDatabase(connectionString)
    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
    .LogToConsole()
    .Build();

var result = upgrader.PerformUpgrade();

if (!result.Successful)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Falha na migração: {result.Error}");
    Console.ResetColor();
    return -1; // Avisa o Aspire que deu erro
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Banco de dados atualizado com sucesso!");
Console.ResetColor();

return 0;