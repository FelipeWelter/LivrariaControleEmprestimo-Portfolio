# Livraria · Controle de empréstimos

Aplicação web em C# com ASP.NET Core MVC, Entity Framework Core e SQL Server. O painel reúne o acervo, os leitores e os empréstimos ativos.

- **Catálogo** — cadastre e edite livros, busque por título ou autor e consulte a disponibilidade.
- **Leitores** — consulte, cadastre, edite e exclua leitores sem histórico de empréstimos.
- **Empréstimos** — registre retiradas e devoluções, filtre os empréstimos em aberto e consulte o histórico.

## Executar no Codespaces

O projeto tem como alvo o .NET 6 e precisa do runtime ASP.NET Core 6. Use um SDK capaz de compilar projetos `net6.0` e um SQL Server de teste. Exemplo para um Codespace com Docker:

```bash
export MSSQL_SA_PASSWORD="DemoAa1_$(openssl rand -hex 16)"
docker run -d --name livraria-sql-demo \
  -e ACCEPT_EULA=Y -e MSSQL_SA_PASSWORD \
  -p 127.0.0.1:14330:1433 \
  mcr.microsoft.com/mssql/server:2022-latest
```

Quando o SQL Server estiver pronto, execute no mesmo terminal:

```bash
export LIVRARIA_CONNECTION_STRING="Server=127.0.0.1,14330;Database=LivrariaDemo;User Id=sa;Password=$MSSQL_SA_PASSWORD;TrustServerCertificate=True"
export ASPNETCORE_ENVIRONMENT=Development
dotnet run --project LivrariaControleEmprestimo.WEB \
  --no-launch-profile --urls http://0.0.0.0:5000
```

Se já houver um contêiner de demonstração, reutilize sua porta e senha em vez de executar `docker run` novamente. O EF Core cria o esquema no primeiro uso em desenvolvimento. Abra a porta privada `5000` pela aba **Portas**.

## Demonstração rápida

1. Cadastre **Dom Casmurro**, de **Machado de Assis**, no catálogo.
2. Cadastre um leitor fictício na tela **Leitores**.
3. Registre o empréstimo e verifique se o livro aparece como **Emprestado** no catálogo.
4. Registre a devolução e confirme se o livro volta a ficar **Disponível**. O registro permanece no histórico.
