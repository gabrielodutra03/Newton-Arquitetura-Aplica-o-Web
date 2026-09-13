using System.Text;
using System.Text.Json;
using LojaDev.Compartilhado.Fila;
using LojaDev.Compartilhado.Modelos;

namespace LojaDev.LojaApi.Servicos;

public class NotificacaoBackground : BackgroundService
{
    private readonly FilaDePedidos _fila;
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _config;

    public NotificacaoBackground(
        FilaDePedidos fila,
        IHttpClientFactory httpFactory,
        IConfiguration config)
    {
        _fila = fila;
        _httpFactory = httpFactory;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("🔄 NotificacaoBackground iniciado — aguardando eventos na fila...");

        await foreach (var evento in _fila.ConsumirAsync(stoppingToken))
        {
            Console.WriteLine($"📥 [BACKGROUND] Evento recebido — Pedido {evento.PedidoId}");

            await ProcessarEvento(evento);
        }
    }

    private async Task ProcessarEvento(EventoPedidoAprovado evento)
    {
        try
        {
            var urlNotificacao = _config["ServicoNotificacao"];

            var client = _httpFactory.CreateClient();

            var body = new
            {
                Destinatario = evento.Cliente,
                Assunto = $"Pedido {evento.PedidoId} confirmado!",
                Corpo = $"Seu pedido de {evento.Produto} (R$ {evento.Valor}) foi aprovado."
            };

            var json = JsonSerializer.Serialize(body);

            var conteudo = new StringContent(
                json,
                Encoding.UTF8,
                "application/json"
            );

            var resposta = await client.PostAsync(
                $"{urlNotificacao}/api/notificacoes",
                conteudo
            );

            Console.WriteLine(
                resposta.IsSuccessStatusCode
                    ? $"📥 ✅ [BACKGROUND] Notificação enviada — Pedido {evento.PedidoId}"
                    : $"📥 ⚠️ [BACKGROUND] Falha na notificação — Pedido {evento.PedidoId} (status {resposta.StatusCode})"
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"📥 ❌ [BACKGROUND] Erro ao notificar — Pedido {evento.PedidoId}: {ex.Message}"
            );
        }
    }
}
