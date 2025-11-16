# Usuários de Teste

Este projeto cria automaticamente três usuários de teste no startup, caso não existam.

- Normal (Comprador)
  - Email: `usuario@santacasa.local`
  - Senha: `Usuario123A`
  - Tipo de usuário: 1
- Leiloeiro
  - Email: `leiloeiro@santacasa.local`
  - Senha: `Leiloeiro123A`
  - Tipo de usuário: 2
- Administrador
  - Email: `admin@santacasa.local`
  - Senha: `Admin123A`
  - Tipo de usuário: 3

Acesse `http://localhost:5050/Account/Login` para entrar com qualquer uma das credenciais acima.

Observações
- Os usuários de teste são criados com `EmailConfirmed = true`, `Aprovado = true` e `Ativo = true`.
- O seed ocorre durante a inicialização da aplicação.

Alterando credenciais
- Edite os emails e senhas no seed em `Program.cs`.
- Emails configurados: `Program.cs:89-91`.
- Senhas definidas nas chamadas de criação: `Program.cs:109`, `Program.cs:128`, `Program.cs:147`.