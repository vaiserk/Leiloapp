-- Simplified database creation script for Leiloapp
-- Remove UUID extension dependency for simplicity

-- Create tables with simple integer IDs
CREATE TABLE IF NOT EXISTS "AspNetRoles" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(256),
    "NormalizedName" VARCHAR(256),
    "ConcurrencyStamp" TEXT
);

CREATE TABLE IF NOT EXISTS "AspNetUsers" (
    "Id" SERIAL PRIMARY KEY,
    "UserName" VARCHAR(256),
    "NormalizedUserName" VARCHAR(256),
    "Email" VARCHAR(256),
    "NormalizedEmail" VARCHAR(256),
    "EmailConfirmed" BOOLEAN NOT NULL DEFAULT FALSE,
    "PasswordHash" TEXT,
    "SecurityStamp" TEXT,
    "ConcurrencyStamp" TEXT,
    "PhoneNumber" TEXT,
    "PhoneNumberConfirmed" BOOLEAN NOT NULL DEFAULT FALSE,
    "TwoFactorEnabled" BOOLEAN NOT NULL DEFAULT FALSE,
    "LockoutEnd" TIMESTAMP WITH TIME ZONE,
    "LockoutEnabled" BOOLEAN NOT NULL DEFAULT FALSE,
    "AccessFailedCount" INTEGER NOT NULL DEFAULT 0,
    "CPF" VARCHAR(14) NOT NULL,
    "Nome" VARCHAR(100) NOT NULL,
    "DataNascimento" DATE,
    "Endereco" VARCHAR(200),
    "Telefone" VARCHAR(20),
    "TipoUsuario" INTEGER NOT NULL DEFAULT 0,
    "DataCadastro" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "Ativo" BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE IF NOT EXISTS "AspNetRoleClaims" (
    "Id" SERIAL PRIMARY KEY,
    "RoleId" INTEGER NOT NULL REFERENCES "AspNetRoles"("Id") ON DELETE CASCADE,
    "ClaimType" TEXT,
    "ClaimValue" TEXT
);

CREATE TABLE IF NOT EXISTS "AspNetUserClaims" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" INTEGER NOT NULL REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE,
    "ClaimType" TEXT,
    "ClaimValue" TEXT
);

CREATE TABLE IF NOT EXISTS "AspNetUserLogins" (
    "LoginProvider" VARCHAR(128) NOT NULL,
    "ProviderKey" VARCHAR(128) NOT NULL,
    "ProviderDisplayName" TEXT,
    "UserId" INTEGER NOT NULL REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE,
    PRIMARY KEY ("LoginProvider", "ProviderKey")
);

CREATE TABLE IF NOT EXISTS "AspNetUserRoles" (
    "UserId" INTEGER NOT NULL REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE,
    "RoleId" INTEGER NOT NULL REFERENCES "AspNetRoles"("Id") ON DELETE CASCADE,
    PRIMARY KEY ("UserId", "RoleId")
);

CREATE TABLE IF NOT EXISTS "AspNetUserTokens" (
    "UserId" INTEGER NOT NULL REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE,
    "LoginProvider" VARCHAR(128) NOT NULL,
    "Name" VARCHAR(128) NOT NULL,
    "Value" TEXT,
    PRIMARY KEY ("UserId", "LoginProvider", "Name")
);

-- Leilao (Auction) table
CREATE TABLE IF NOT EXISTS "Leiloes" (
    "Id" SERIAL PRIMARY KEY,
    "Titulo" VARCHAR(200) NOT NULL,
    "Descricao" TEXT,
    "DataInicio" TIMESTAMP WITH TIME ZONE NOT NULL,
    "DataFim" TIMESTAMP WITH TIME ZONE NOT NULL,
    "Local" VARCHAR(200),
    "Status" INTEGER NOT NULL DEFAULT 0,
    "LeiloeiroId" INTEGER NOT NULL REFERENCES "AspNetUsers"("Id"),
    "CriadoEm" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "AtualizadoEm" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Lote (Lot) table
CREATE TABLE IF NOT EXISTS "Lotes" (
    "Id" SERIAL PRIMARY KEY,
    "Numero" INTEGER NOT NULL,
    "Titulo" VARCHAR(200) NOT NULL,
    "Descricao" TEXT,
    "ValorInicial" DECIMAL(18,2) NOT NULL,
    "ValorMinimo" DECIMAL(18,2),
    "LanceAtual" DECIMAL(18,2),
    "LanceMinimo" DECIMAL(18,2),
    "LanceVencedorId" INTEGER REFERENCES "AspNetUsers"("Id"),
    "Status" INTEGER NOT NULL DEFAULT 0,
    "LeilaoId" INTEGER NOT NULL REFERENCES "Leiloes"("Id") ON DELETE CASCADE,
    "CriadoEm" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "AtualizadoEm" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Lance (Bid) table
CREATE TABLE IF NOT EXISTS "Lances" (
    "Id" SERIAL PRIMARY KEY,
    "Valor" DECIMAL(18,2) NOT NULL,
    "DataHora" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "LoteId" INTEGER NOT NULL REFERENCES "Lotes"("Id") ON DELETE CASCADE,
    "UsuarioId" INTEGER NOT NULL REFERENCES "AspNetUsers"("Id"),
    "Vencedor" BOOLEAN NOT NULL DEFAULT FALSE
);

-- LoteImagem (Lot Image) table
CREATE TABLE IF NOT EXISTS "LoteImagens" (
    "Id" SERIAL PRIMARY KEY,
    "Url" VARCHAR(500) NOT NULL,
    "Descricao" VARCHAR(200),
    "Ordem" INTEGER NOT NULL DEFAULT 0,
    "Principal" BOOLEAN NOT NULL DEFAULT FALSE,
    "LoteId" INTEGER NOT NULL REFERENCES "Lotes"("Id") ON DELETE CASCADE
);

-- Pagamento (Payment) table
CREATE TABLE IF NOT EXISTS "Pagamentos" (
    "Id" SERIAL PRIMARY KEY,
    "Valor" DECIMAL(18,2) NOT NULL,
    "DataPagamento" TIMESTAMP WITH TIME ZONE,
    "Status" INTEGER NOT NULL DEFAULT 0,
    "MetodoPagamento" INTEGER NOT NULL,
    "CompradorId" INTEGER NOT NULL REFERENCES "AspNetUsers"("Id"),
    "LoteId" INTEGER NOT NULL REFERENCES "Lotes"("Id"),
    "CriadoEm" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Notificacao (Notification) table
CREATE TABLE IF NOT EXISTS "Notificacoes" (
    "Id" SERIAL PRIMARY KEY,
    "Titulo" VARCHAR(200) NOT NULL,
    "Mensagem" TEXT NOT NULL,
    "Tipo" INTEGER NOT NULL,
    "Lida" BOOLEAN NOT NULL DEFAULT FALSE,
    "DataCriacao" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UsuarioId" INTEGER NOT NULL REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE
);

-- Create basic indexes for performance
CREATE INDEX IF NOT EXISTS "IX_AspNetUsers_NormalizedEmail" ON "AspNetUsers"("NormalizedEmail");
CREATE INDEX IF NOT EXISTS "IX_AspNetUsers_NormalizedUserName" ON "AspNetUsers"("NormalizedUserName");
CREATE INDEX IF NOT EXISTS "IX_Leiloes_LeiloeiroId" ON "Leiloes"("LeiloeiroId");
CREATE INDEX IF NOT EXISTS "IX_Lotes_LeilaoId" ON "Lotes"("LeilaoId");
CREATE INDEX IF NOT EXISTS "IX_Lances_LoteId" ON "Lances"("LoteId");
CREATE INDEX IF NOT EXISTS "IX_Lances_UsuarioId" ON "Lances"("UsuarioId");
CREATE INDEX IF NOT EXISTS "IX_LoteImagens_LoteId" ON "LoteImagens"("LoteId");
CREATE INDEX IF NOT EXISTS "IX_Pagamentos_CompradorId" ON "Pagamentos"("CompradorId");
CREATE INDEX IF NOT EXISTS "IX_Pagamentos_LoteId" ON "Pagamentos"("LoteId");
CREATE INDEX IF NOT EXISTS "IX_Notificacoes_UsuarioId" ON "Notificacoes"("UsuarioId");

-- Insert default admin user (password: Admin@123)
INSERT INTO "AspNetUsers" ("UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "CPF", "Nome", "DataNascimento", "TipoUsuario", "Ativo")
VALUES ('admin', 'ADMIN', 'admin@leiloapp.com', 'ADMIN@LEILOAPP.COM', true, 'AQAAAAIAAYagAAAAEM+teste+hash+senha+Admin@123', 'teste-stamp', 'teste-concurrency', '000.000.000-00', 'Administrador', '1990-01-01', 3, true);