# Leiloapp — Documentação Técnica e Funcional

## Sumário
- Visão Geral
- Tecnologias
- Requisitos
- Instalação e Execução
- Arquitetura e Estrutura
- Módulos e Fluxos
- Banco de Dados
- Regras de Negócio
- Tempo Real (SignalR)
- Dashboard e Métricas
- Segurança e Usuários
- Logs e Observabilidade
- Convenções de Código
- Processo de Versionamento (Git)
- Capturas de Tela

## Visão Geral
Leiloapp é um sistema de leilões com foco em gestão de leilões, lotes e lances, incluindo acompanhamento em tempo real, cálculo de arrecadação e dashboards para administração e leiloeiro.

Principais capacidades:
- Cadastro e gestão de leilões e lotes
- Lances em tempo real e sala de leilão
- Finalização de lotes (vendido / não vendido) com atualização automática da UI
- Cálculo de arrecadação por leilão e geral
- Dashboard com gráficos (pizza e barras) e estatísticas

## Tecnologias
- .NET `net9.0` — `c:\Projetos\Leiloapp\Leiloapp.csproj:4`
- ASP.NET Core MVC
- ASP.NET Core Identity (usuarios em `AspNetUsers`)
- Entity Framework Core 9
- PostgreSQL via `Npgsql.EntityFrameworkCore.PostgreSQL` — `Leiloapp.csproj:16`
- SignalR para tempo real — `Program.cs:62`, `Program.cs:184`
- Serilog com sink para PostgreSQL — `Leiloapp.csproj:17–19`
- Bootstrap 5, Chart.js (via CDN) nos dashboards

## Requisitos
- .NET SDK 9
- PostgreSQL acessível via connection string `PostgreSQL` (em `appsettings.json`)
- Windows ou Linux com suporte a .NET 9

## Instalação e Execução
- Restaurar e executar:
  - `dotnet run`
- Aplicação inicia em `http://localhost:5266/` (Development) — ver `Program.cs:185–190`
- Seed mínimo de usuários de teste é criado se não existir — `Program.cs:108–169`
  - Leiloeiro: `leiloeiro@santacasa.local`
  - Admin: `admin@santacasa.local`
  - Usuário: `usuario@santacasa.local`

## Arquitetura e Estrutura
- Camadas principais:
  - Controllers MVC (`Controllers/*`)
  - Views Razor (`Views/*`)
  - Serviços (`Services/*`) e interfaces (`Services/Interfaces/*`)
  - Hub de tempo real (`Hubs/LeilaoHub.cs`)
  - Contexto de dados (`Data/ApplicationDbContext.cs`)
- Rotas padrão: `Program.cs:186–190`
- Hub mapeado em `/hubs/leilao` — `Program.cs:184`

## Módulos e Fluxos
- Leilões
  - CRUD, detalhe incl. lotes e arrecadação recalculada ao carregar
  - Recalcular arrecadação em detalhe do leilão: `Controllers/LeilaoController.cs:62`
- Lotes
  - Cadastro e edição de lotes (valor mínimo, título, descrição)
  - Abertura para lances (habilita botões de finalização na UI)
  - Finalização vendido / não vendido (atualiza card em tempo real)
- Lance / Sala do Leilão
  - Lances em tempo real via SignalR
  - Atualização instantânea do lance atual e status de lote
- Dashboard
  - Admin: gráficos pizza e barras, contagens e totais
  - Leiloeiro: visão de seus leilões e totais individuais

## Banco de Dados
- Provider: PostgreSQL
- Garantias na inicialização:
  - Criação do banco: `Program.cs:99`
  - Alteração de FK `Lotes -> Leiloes` para `ON DELETE CASCADE`: `Program.cs:103`
  - Coluna `Lotes.Visivel` adicionada se não existir: `Program.cs:104`
- Configuração de cascade também no EF: `Data/ApplicationDbContext.cs:125` (`DeleteBehavior.Cascade`)

## Regras de Negócio
- Exclusão de leilão remove lotes em cascata
- Arrecadação
  - Soma `LanceAtual` dos lotes com `Status == 2` (vendidos)
  - Atualizada em detalhe do leilão: `LeilaoController.cs:62`
  - Dashboard usa somas diretas de lotes vendidos
- Status de lote
  - `0`: não iniciado
  - `1`: em licitação
  - `2`: vendido
  - `3`: não vendido

## Tempo Real (SignalR)
- Eventos de servidor (exemplos):
  - `LoteAberto` — dispara ao abrir lote para lances — `Controllers/LanceApiController.cs:132`
  - `LoteFinalizado` — dispara ao finalizar lote — `Controllers/LanceApiController.cs:168–175`
  - `LeilaoAtualizado` — atualiza totais — `Controllers/LanceApiController.cs:174–175`
- Clientes consomem:
  - `Views/Leilao/Details.cshtml:138–146` (`LeilaoAtualizado`)
  - `Views/Leilao/Details.cshtml:146–163` (`LoteAberto`)
  - `Views/Leilao/Details.cshtml:143–158` (`LoteFinalizado`)

## Dashboard e Métricas
- Admin
  - Pizza: distribuição de lotes (vendidos, não vendidos, não iniciados, em licitação)
  - Barras: comparação por lote entre `Valor Mínimo` e `Valor Vendido` (Top 10)
  - ViewModel estendido: `Controllers/DashboardController.cs:36–56`
  - Dados preenchidos: `Controllers/DashboardController.cs:66–78`
  - Renderização Chart.js: `Views/Dashboard/Admin.cshtml:48–92`
- Leiloeiro
  - Total arrecadado por leilão do usuário
  - Somatórios e filtros específicos

## Segurança e Usuários
- ASP.NET Identity configurado: `Program.cs:35–52`
- Políticas de senha e lockout configuradas: `Program.cs:36–49`
- Cookie de autenticação com paths: `Program.cs:55–59`
- Tipos de usuário em `Usuario.TipoUsuario` (1: usuário, 2: leiloeiro, 3: admin)

## Logs e Observabilidade
- Serilog configurado via `appsettings.json` e `Program.cs:15–20`
- Sink PostgreSQL habilitado (ver `Leiloapp.csproj:17–19` e config no `appsettings.json`)

## Convenções de Código
- Cultura padrão `pt-BR` aplicada ao request e threads — `Program.cs:23–29`, `Program.cs:91–94`
- Usar EF Core com `Include` para carregar relacionamentos
- Evitar persistir dados em ações `GET` (ex.: ajuste futuro para cálculo de arrecadação sem `SaveChanges`)
- Front-end com Bootstrap 5, sem dependências locais de Chart.js (via CDN)

## Processo de Versionamento (Git)
- Branch principal: `main`
- Recomendado:
  - Criar branches de feature: `feature/dashboard-graficos`, `feature/realtime-lote`, etc.
  - Pull Request para `main` com revisão
  - Tags de release: `vX.Y.Z`
- Exemplo de fluxo:
  - `git checkout -b feature/dashboard-graficos`
  - implementar e commitar
  - `git push -u origin feature/dashboard-graficos`
  - abrir PR para `main`

## Capturas de Tela
Substituir os placeholders abaixo por imagens reais (PNG/JPG) adicionando-as em `docs/img/` e atualizando os caminhos.

- Home: `![Home](../docs/img/home.png)`
- Leilões (lista): `![Leilões](../docs/img/leiloes-lista.png)`
- Detalhe do Leilão com lotes: `![Detalhe Leilão](../docs/img/leilao-detalhe.png)`
- Sala do Leilão (tempo real): `![Sala Leilão](../docs/img/sala-leilao.png)`
- Dashboard Admin (pizza): `![Dashboard Pizza](../docs/img/dashboard-pizza.png)`
- Dashboard Admin (barras): `![Dashboard Barras](../docs/img/dashboard-barras.png)`

---

## Anexo: Rotas e Páginas Úteis
- `GET /` — Home
- `GET /Leilao` — Lista de Leilões
- `GET /Leilao/Details/{id}` — Detalhes do Leilão
- `GET /Dashboard/Admin` — Dashboard do Admin
- `GET /Dashboard/Leiloeiro` — Dashboard do Leiloeiro
- `POST /api/lance/abrir-lote` — Abrir lote para lances
- `POST /api/lance/finalizar-lote` — Finalizar lote vendido / não vendido

## Anexo: Referências de Código
- `Program.cs:95–106` — inicialização de banco, alterações de FK e coluna
- `Program.cs:108–169` — seed de usuários de teste
- `Data/ApplicationDbContext.cs:125` — cascade EF em `Lote → Leilao`
- `Views/Shared/_Layout.cshtml:36–46` — exibição do nome do usuário logado
- `Views/Leilao/Details.cshtml:138–181` — eventos de tempo real e atualizações de card
- `Views/Dashboard/Admin.cshtml:24–92` — containers e script dos gráficos
- `Controllers/DashboardController.cs:36–78` — dados para gráficos e métricas