using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Leiloapp.Models.Entities;
using System;

namespace Leiloapp.Data
{
    public class ApplicationDbContext : IdentityDbContext<Usuario, Microsoft.AspNetCore.Identity.IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Leilao> Leiloes { get; set; }
        public DbSet<Lote> Lotes { get; set; }
        public DbSet<Lance> Lances { get; set; }
        public DbSet<Pagamento> Pagamentos { get; set; }
        public DbSet<LoteImagem> LoteImagens { get; set; }
        public DbSet<Notificacao> Notificacoes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração da tabela de usuários
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.CPF).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.TipoUsuario);
                entity.HasIndex(e => e.Aprovado);

                entity.Property(e => e.CPF)
                    .IsRequired()
                    .HasMaxLength(14);

                entity.Property(e => e.Nome)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.TipoUsuario)
                    .IsRequired()
                    .HasDefaultValue(0);

                entity.Property(e => e.DataCadastro)
                    .HasDefaultValueSql("NOW()");
            });

            // Configuração da tabela de leilões
            modelBuilder.Entity<Leilao>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.DataInicio);
                entity.HasIndex(e => e.DataFim);

                entity.Property(e => e.Titulo)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasDefaultValue(0);

                entity.Property(e => e.CriadoEm)
                    .HasDefaultValueSql("NOW()");

                entity.Property(e => e.AtualizadoEm)
                    .HasDefaultValueSql("NOW()");

                entity.HasOne(e => e.Leiloeiro)
                    .WithMany(u => u.LeiloesAdministrados)
                    .HasForeignKey(e => e.LeiloeiroId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuração da tabela de lotes
            modelBuilder.Entity<Lote>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.LeilaoId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.LanceVencedorId);

                entity.Property(e => e.Titulo)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.NomeDoador)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.ValorInicial)
                    .IsRequired()
                    .HasPrecision(18, 2);

                entity.Property(e => e.ValorMinimo)
                    .HasPrecision(18, 2);

                entity.Property(e => e.LanceAtual)
                    .HasDefaultValue(0)
                    .HasPrecision(18, 2);

                entity.Property(e => e.LanceMinimo)
                    .HasPrecision(18, 2);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasDefaultValue(0);

                entity.Property(e => e.Visivel)
                    .HasDefaultValue(true);

                entity.Property(e => e.CriadoEm)
                    .HasDefaultValueSql("NOW()");

                entity.Property(e => e.AtualizadoEm)
                    .HasDefaultValueSql("NOW()");

                entity.HasOne(e => e.Leilao)
                    .WithMany(l => l.Lotes)
                    .HasForeignKey(e => e.LeilaoId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.LanceVencedor)
                    .WithMany(u => u.LotesVencidos)
                    .HasForeignKey(e => e.LanceVencedorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuração da tabela de lances
            modelBuilder.Entity<Lance>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.LoteId);
                entity.HasIndex(e => e.UsuarioId);
                entity.HasIndex(e => e.DataHora);
                entity.HasIndex(e => e.Vencedor);

                entity.Property(e => e.Valor)
                    .IsRequired()
                    .HasPrecision(18, 2);

                entity.Property(e => e.DataHora)
                    .HasDefaultValueSql("NOW()");

                entity.HasOne(e => e.Lote)
                    .WithMany(l => l.Lances)
                    .HasForeignKey(e => e.LoteId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Usuario)
                    .WithMany(u => u.Lances)
                    .HasForeignKey(e => e.UsuarioId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuração da tabela de pagamentos
            modelBuilder.Entity<Pagamento>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Valor)
                    .IsRequired()
                    .HasPrecision(18, 2);

                entity.Property(e => e.MetodoPagamento)
                    .IsRequired();

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasDefaultValue(0);

                entity.Property(e => e.CriadoEm)
                    .HasDefaultValueSql("NOW()");

                entity.HasOne(e => e.Lote)
                    .WithMany(l => l.Pagamentos)
                    .HasForeignKey(e => e.LoteId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Comprador)
                    .WithMany(u => u.Pagamentos)
                    .HasForeignKey(e => e.CompradorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuração da tabela de imagens dos lotes
            modelBuilder.Entity<LoteImagem>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Url)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.Descricao)
                    .HasMaxLength(200);

                entity.HasOne(e => e.Lote)
                    .WithMany(l => l.Imagens)
                    .HasForeignKey(e => e.LoteId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuração da tabela de notificações
            modelBuilder.Entity<Notificacao>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UsuarioId);

                entity.Property(e => e.Titulo)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Mensagem)
                    .IsRequired();

                entity.Property(e => e.Tipo)
                    .IsRequired();

                entity.Property(e => e.DataCriacao)
                    .HasDefaultValueSql("NOW()");

                entity.HasOne(e => e.Usuario)
                    .WithMany(u => u.Notificacoes)
                    .HasForeignKey(e => e.UsuarioId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}