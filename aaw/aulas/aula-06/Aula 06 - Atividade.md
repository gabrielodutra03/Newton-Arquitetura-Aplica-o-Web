# Atividade — AULA 06

## Síncrono ou Assíncrono?

*Análise de fluxos de comunicação entre serviços — Arquitetura de Aplicações Web*

## 🎯 MISSÃO

Vocês são os arquitetos dos 4 fluxos abaixo. Para CADA cenário:

- Decidam o estilo de comunicação: síncrono (request/response), assíncrono (fila/evento) ou API Gateway/BFF
- Desenhem o fluxo com caixas (serviços) e setas (chamadas/mensagens) no espaço indicado
- Justifiquem com pelo menos 2 fatores (urgência da resposta, tolerância a atraso, picos, falhas...)
- Apontem o principal risco da escolha de vocês

*⏱️ Tempo: 25 minutos  |  👥 Formato: em duplas  |  Não existe resposta única — o que vale é a justificativa.*

> **Nomes:** _____Gabriel Otone_______________   **Turma:** ____Sistemas da informação________________   **Data:** _12__ / _09__ / _2026_____

## CENÁRIO 01 — PagFácil — aprovar ou negar AGORA

No checkout do PagFácil, ao clicar em “Pagar”, o serviço de Pagamentos precisa consultar o saldo/limite do cliente no serviço de Contas — e a resposta define se a venda acontece neste exato momento.

- O cliente está na tela, esperando o resultado da compra
- Sem a resposta de Contas, não há decisão possível: aprovar às cegas é proibido
- Tempo de resposta do serviço de Contas: ~80 ms em condições normais

**Sua análise:**

1. Estilo recomendado:   x Síncrono      ☐ Assíncrono (fila/evento)      ☐ API Gateway/BFF

2. Desenhe o fluxo (caixas = serviços, setas = chamadas/mensagens):

[Cliente]
    ↓
[Pagamentos]
    ↓ consulta
[Contas]
    ↓ resposta
[Pagamentos]
    ↓
[Cliente]

3. Justificativa (mínimo 2 fatores):

Devido ao cliente precisar de uma resposta imediata. Além disso, o tempo de resposta é rapido, de aproximadamente 80 ms.

4. Principal risco da escolha:

O principal risco é o serviço de Contas ficar indisponível ou lento, fazendo o pagamento também ficar indisponível ou demorar para responder.

## CENÁRIO 02 — CadastraJá — o e-mail de boas-vindas

Após criar a conta no CadastraJá, o sistema envia um e-mail de boas-vindas. O provedor de e-mail às vezes demora 8 segundos para responder e falha em 2% das tentativas.

- O usuário quer começar a usar o app imediatamente após o cadastro
- O e-mail chegar 1 minuto depois não incomoda ninguém
- Se o provedor falhar, o envio deve ser tentado de novo — sem o usuário perceber

**Sua análise:**

1. Estilo recomendado:   ☐ Síncrono      x Assíncrono (fila/evento)      ☐ API Gateway/BFF

2. Desenhe o fluxo (caixas = serviços, setas = chamadas/mensagens):

[Usuário]
    ↓
[CadastraJá]
    ↓
[Cria conta]
    ↓
[Fila de mensagens]
    ↓
[Serviço de E-mail]
    ↓
[Provedor de E-mail]

3. Justificativa (mínimo 2 fatores):

O usuário não precisa esperar o e-mail para começar a utilizar o sistema e a fila permite realizar novas tentativas em caso de erro.

4. Principal risco da escolha:

O principal risco é a mensagem ficar presa ou ser perdida na fila, fazendo o usuário não receber o e-mail.

## CENÁRIO 03 — MegaMarket — baixa de estoque nos picos

No marketplace MegaMarket, cada venda gera uma baixa no serviço de Estoque. Nas grandes promoções o tráfego sobe 10x e o Estoque não dá conta de responder na velocidade das vendas.

- Atraso de alguns segundos na baixa é aceitável
- PERDER uma baixa de estoque não é aceitável (gera venda sem produto)
- O checkout não pode ficar lento nem cair porque o Estoque está sobrecarregado

**Sua análise:**

1. Estilo recomendado:   ☐ Síncrono      x Assíncrono (fila/evento)      ☐ API Gateway/BFF

2. Desenhe o fluxo (caixas = serviços, setas = chamadas/mensagens):

[Cliente]
    ↓
[Checkout]
    ↓
[Venda aprovada]
    ↓
[Fila de Estoque]
    ↓
[Serviço de Estoque]
    ↓
[Baixa do produto]

3. Justificativa (mínimo 2 fatores):

O checkout não precisa esperar alguns segundos pela baixa do estoque e a mensagem pode ser processada posteriomente sem perder informações.

4. Principal risco da escolha:

O principal risco é ocorrer uma falha ou duplicação na mensagem, causando inconsistência no estoque


## CENÁRIO 04 — AppBanco — uma tela, cinco serviços

A tela inicial do AppBanco mostra saldo, fatura do cartão, investimentos, empréstimos e cashback — dados de 5 serviços diferentes. O time mobile reclama: são 5 chamadas, 5 formatos de resposta e 5 pontos de falha em cada abertura do app.

- A tela precisa abrir rápido, inclusive em redes móveis ruins
- Cada serviço tem equipe, formato e autenticação próprios
- Amanhã nasce a versão web, que precisa de MAIS dados que a mobile

**Sua análise:**

1. Estilo recomendado:   ☐ Síncrono      ☐ Assíncrono (fila/evento)      x API Gateway/BFF

2. Desenhe o fluxo (caixas = serviços, setas = chamadas/mensagens):

                 ┌→ [Saldo]
                 │
                 ├→ [Cartão]
[App Mobile] → [BFF] ├→ [Investimentos]
                 │
                 ├→ [Empréstimos]
                 │
                 └→ [Cashback]
                       ↓
                  [Resposta única]
                       ↓
                    [App]

3. Justificativa (mínimo 2 fatores):

Reduz a quantidade de chamadas que o aplicativo precisa fazer diretamente e centraliza parte da autenticação e comunicação com os serviços.

4. Principal risco da escolha:

O principal risco é o BFF se tornar um ponto único de falha ou ficar sobrecarregado. Se ele apresentar problemas, várias funcionalidades da tela podem ser afetadas.

## DESAFIO

1. Escolha um cenário em que vocês indicaram ASSÍNCRONO. Os brokers de mensagens costumam garantir entrega “pelo menos uma vez” — ou seja, a MESMA mensagem pode chegar duas vezes. O que aconteceria no seu fluxo? Como o consumidor deveria se proteger?

Cenário 03- 

Se o broker entregar a mesma mensagem duas vezes, o serviço de Estoque poderia realizar a baixa do mesmo produto duas vezes, causando um estoque incorreto.

Para evitar isso, o consumidor deve ser idempotente, verificando se aquele evento já foi processado antes de realizar a baixa.

Assim, mesmo que a mesma mensagem seja entregue duas vezes, o estoque será alterado apenas uma vez.
