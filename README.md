# Leiloapp — Sistema de Leilões

Leiloapp é uma aplicação web para gerenciamento e condução de leilões com acompanhamento em tempo real. O projeto utiliza ASP.NET Core 9 (MVC), SignalR, Entity Framework Core (PostgreSQL) e ASP.NET Identity para autenticação, além de Serilog para observabilidade.

## Visão Geral
- Plataforma de leilões com cadastro de leilões, lotes e lances.
- Atualizações em tempo real via SignalR para lances e status de lotes/leilões.
- Autenticação baseada em cookie com ASP.NET Identity e aprovação de usuários.
- Persistência em PostgreSQL via EF Core e logging com Serilog.

## Stack Técnica
- .NET `net9.0` (`Leiloapp.csproj`)
- ASP.NET Core MVC + SignalR (`Program.cs`)
- Entity Framework Core + `Npgsql` (PostgreSQL)
- ASP.NET Identity com `Usuario` derivado de `IdentityUser<int>`
- Serilog (saída em console e arquivo)
- FluentValidation para validações

## Requisitos
- .NET SDK 9
- PostgreSQL 15+ (ou compatível)
- Acesso a `localhost` na porta configurada para o banco (padrão do projeto: `5433`)

## Configuração
- Arquivo `appsettings.json`:
  - `ConnectionStrings.PostgreSQL`: configure host, porta, banco, usuário e senha.
  - `Serilog`: já configurado para Console e arquivo em `logs/leiloapp-.txt`.
  - `JwtSettings`: presente, mas não utilizado na autenticação atual (cookie-based). Reserve para uso futuro.

Exemplo (padrão do projeto):
```json
{
  "ConnectionStrings": {
    "PostgreSQL": "Server=localhost;Port=5433;Database=leiloapp;User Id=postgres;Password=123;"
  }
}
```

> Segurança: nunca versionar segredos reais. Use variáveis de ambiente ou secrets do .NET em produção.

## Como Executar
1. Instale dependências do .NET e garanta o PostgreSQL ativo.
2. Ajuste a string de conexão em `appsettings.json` se necessário.
3. Rode a aplicação:
   ```bash
   dotnet restore
   dotnet run
   ```
4. A aplicação inicia em:
   - HTTP: `http://localhost:5266`
   - HTTPS: `https://localhost:7144`

Ambiente de desenvolvimento é definido em `Properties/launchSettings.json`. Banco é criado automaticamente no primeiro run (`EnsureCreated`).

## Banco de Dados e Seed
- O banco é garantido/ajustado na inicialização com `EnsureCreated` e pequenas migrações SQL para colunas/constraints.
- Usuários de teste são semeados automaticamente (Administrador, Leiloeiro e Comprador). Veja `docs/usuarios-teste.md` para credenciais.

Detalhes técnicos:
- Criação do banco e ajustes: `Program.cs`
- Seed de usuários: `Program.cs` (emails e senhas padrão)

## Autenticação e Autorização
- Autenticação baseada em cookie via ASP.NET Identity.
- Políticas de senha exigem dígito, maiúscula/minúscula e tamanho mínimo 8.
- Fluxos:
  - Login: `GET/POST /Account/Login`
  - Registro: `GET/POST /Account/Register`
  - Logout: `POST /Account/Logout`
- Perfis com `TipoUsuario`:
  - `1` Comprador, `2` Leiloeiro, `3` Administrador
- Restrições de acesso em controllers e views checam `TipoUsuario` e `[Authorize]`.

## Tempo Real (SignalR)
- Hub mapeado em `/hubs/leilao`.
- Grupos por leilão e lote (`leilao-{id}`, `lote-{id}`).
- Eventos enviados incluem `NovoLance`, `LoteAberto`, `LoteFinalizado`, `LeilaoAtualizado`, entre outros.

## API de Lances
Endpoints (autenticados):
- `POST /api/lance/dar-lance`
  - Body: `{ "LoteId": number, "Valor": number }`
  - Efeitos: atualiza lote, marca lance vencedor e emite `NovoLance` via SignalR.
- `POST /api/lance/abrir-lote`
  - Body: `{ "LoteId": number }`
  - Requer perfil Leiloeiro. Atualiza status do lote para aberto e emite `LoteAberto`.
- `POST /api/lance/finalizar-lote`
  - Body: `{ "LoteId": number, "Vendido": boolean }`
  - Requer perfil Leiloeiro. Finaliza lote, atualiza arrecadação do leilão e emite `LoteFinalizado` e `LeilaoAtualizado`.

## Estrutura de Pastas
- `Controllers/`: MVC controllers e API.
- `Models/Entities/`: entidades de domínio (`Leilao`, `Lote`, `Lance`, `Usuario` etc.).
- `Data/`: `ApplicationDbContext` e configuração EF.
- `Services/` e `Services/Interfaces/`: lógica de domínio (leilões, lances, usuários).
- `Hubs/`: SignalR hubs.
- `Views/`: páginas Razor.
- `wwwroot/`: assets estáticos.
- `docs/`: documentação adicional (ex.: usuários de teste).

## Logs
- Serilog grava em `logs/leiloapp-YYYYMMDD.txt` e Console.
- Ajuste níveis em `appsettings*.json`.

## CORS
- Política `AllowLocalhost` permite origens `http://localhost:3000`/`https://localhost:3000` para integrações frontend.

## Convenções e Boas Práticas
- Não armazenar segredos reais em `appsettings.json`.
- Validar inputs (FluentValidation disponível).
- Respeitar regras de negócio nos services e controllers.
- Em produção, substituir `EnsureCreated` por migrações EF (`dotnet ef migrations`).

## Próximos Passos
- Habilitar JWT caso exponha APIs públicas.
- Adicionar migrações EF e pipeline CI/CD.
- Ampliar testes automatizados.