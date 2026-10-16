# SM.MAF.Demo

Projeto de estudo sobre **embeddings e busca semântica** em uma API **ASP.NET Core 10**, usando **Ollama** para gerar vetores e **PostgreSQL com pgvector** para armazenar e consultar recomendações de cafés.

## 🎯 O que demonstra

- Geração local de embeddings com o modelo `mxbai-embed-large`.
- Integração entre OllamaSharp e Semantic Kernel.
- Persistência de vetores de 1024 dimensões com Entity Framework Core e pgvector.
- Busca por similaridade usando distância de cosseno, retornando até três recomendações.
- Inicialização do PostgreSQL e carga de produtos de exemplo com Docker Compose.
- Minimal APIs com validação de título, categoria e prompt.

O nome do repositório acompanha os estudos sobre Microsoft Agent Framework. A implementação atual explora a geração e a consulta de embeddings com Semantic Kernel e OllamaSharp; ainda não possui um agente MAF.

## 🛠️ Tecnologias

`C#` · `.NET 10` · `ASP.NET Core` · `Entity Framework Core` · `PostgreSQL 17` · `pgvector` · `Ollama` · `Semantic Kernel` · `Docker Compose`

## 🏗️ Estrutura do projeto

```text
SM.MAF.Demo/
├── Infra/
│   ├── Data/AppDbContext.cs   # Mapeamento das entidades e da coluna vetorial
│   ├── Scripts/
│   │   ├── schema.sql        # Extensão, tabelas e índice HNSW para cosseno
│   │   ├── seed.sql          # Produtos de exemplo
│   │   └── script.sql        # Executa o esquema e a carga de produtos
│   └── compose.yaml          # PostgreSQL e serviço de inicialização
├── Models/                   # Product e Recomendation
├── ViewModels/               # Dados de entrada dos endpoints
├── Program.cs                # Configuração e endpoints da API
└── appsettings.json          # Connection string e logs
```

## 🚀 Como executar

Pré-requisitos: [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0), [Docker com Compose](https://docs.docker.com/compose/) e [Ollama](https://ollama.com/).

### 1. Obter o projeto

```bash
git clone https://github.com/SousaMatheus/SM.MAF.Demo.git
cd SM.MAF.Demo
```

### 2. Preparar o Ollama

Com o Ollama em execução, baixe o modelo:

```bash
ollama pull mxbai-embed-large
```

A aplicação espera o Ollama em `http://localhost:11434`. Se ele ainda não estiver iniciado, execute `ollama serve` em outro terminal.

### 3. Preparar o banco

```bash
docker compose -f Infra/compose.yaml up -d
docker compose -f Infra/compose.yaml logs db-init
```

O serviço `db-init` aguarda o banco ficar disponível, cria o esquema e insere os 15 produtos de exemplo. Ele encerra após a execução; o PostgreSQL permanece ativo. Confira nos logs se a inicialização terminou sem erros antes de continuar.

No Windows, se o Docker estiver disponível apenas no WSL, acrescente `wsl` antes dos comandos `docker compose`.

A conexão local já está configurada em `appsettings.json`:

```text
Host=localhost;Port=5432;Database=sm_maf_demo;Username=demo;Password=demo;
```

Os dados ficam no volume `postgres_data`. O usuário e a senha são valores de demonstração para uso local.

### 4. Iniciar a API

```bash
dotnet restore
dotnet run --launch-profile http
```

A API ficará disponível em `http://localhost:5270`. Os exemplos abaixo usam PowerShell.

### 5. Gerar as recomendações iniciais

```powershell
Invoke-RestMethod -Method Post -Uri 'http://localhost:5270/api/v1/seed'
```

Esse endpoint lê a tabela `products`, gera um embedding da categoria de cada produto e grava título, categoria e vetor em `recomendations`.

### 6. Consultar recomendações

```powershell
$body = @{ prompt = 'I want an intense coffee' } | ConvertTo-Json
Invoke-RestMethod -Method Post -Uri 'http://localhost:5270/api/v1/recomendation' -ContentType 'application/json' -Body $body
```

A consulta transforma o prompt em um vetor e retorna até três registros, ordenados pela menor distância de cosseno, com `title` e `category`.

## 📡 Endpoints

| Endpoint | Comportamento | Resposta de sucesso |
| --- | --- | --- |
| `POST /api/v1/seed` | Gera recomendações a partir dos produtos existentes | `200 OK` com mensagem `OK` |
| `POST /api/v1/product` | Insere uma recomendação com embedding da categoria informada | `201 Created`, sem corpo |
| `POST /api/v1/recomendation` | Busca as três recomendações mais próximas do prompt | `200 OK` com uma lista |

Exemplo de corpo para `POST /api/v1/product`:

```json
{
  "id": 0,
  "title": "Example Coffee",
  "category": "intense",
  "summary": "A full-bodied coffee",
  "description": "Coffee with an intense flavor."
}
```

O contrato recebe esses campos, mas a implementação atual utiliza apenas `title` e `category` para criar um registro em `recomendations`. Ela não insere em `products`; `id`, `summary` e `description` não são persistidos por esse endpoint.

Título ou categoria em branco no cadastro, assim como um prompt em branco na consulta, retornam `400 Bad Request`.

## 📚 Observações do estudo

- Os embeddings dos produtos usam apenas a categoria. Produtos com a mesma categoria têm o mesmo texto de entrada e não são diferenciados pelo título ou pela descrição na busca.
- A consulta retorna os registros mais próximos disponíveis, sem um limite mínimo de similaridade.
- O seed de recomendações não verifica registros existentes: chamadas repetidas podem duplicar os dados. A reexecução do seed SQL também insere novamente os produtos de exemplo.
- `CREATE TABLE IF NOT EXISTS` e `CREATE INDEX IF NOT EXISTS` não atualizam objetos existentes. Alterações no esquema ou no tipo do índice exigem aplicação explícita no banco persistido.
- O projeto não possui migrations nem testes automatizados. A compilação pode ser verificada com `dotnet build`; o fluxo completo depende de PostgreSQL e Ollama em execução.

## 🔗 Referências

- [OllamaSharp](https://github.com/awaescher/OllamaSharp)
- [Modelo mxbai-embed-large](https://ollama.com/library/mxbai-embed-large)
- [pgvector](https://github.com/pgvector/pgvector)
- [pgvector para .NET e Entity Framework Core](https://github.com/pgvector/pgvector-dotnet)
- [Semantic Kernel](https://learn.microsoft.com/semantic-kernel/overview/)

Projeto desenvolvido para estudo, com foco no fluxo de geração, persistência e consulta de embeddings.
