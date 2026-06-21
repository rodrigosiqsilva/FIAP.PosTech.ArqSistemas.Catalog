# FIAP.PosTech.ArqSistemas.CatalogWS.Worker

É serviço de background (**Worker Service**) desenvolvido em **.NET 8** projetado para processar de forma assíncrona os eventos de integração do ecossistema de jogos. 

O objetivo principal deste componente é escutar eventos de pagamentos aprovados trafegados pelo **Apache Kafka**, atualizar o estado interno dos pedidos e vincular automaticamente os jogos adquiridos à biblioteca digital do jogador comprador.

---

## 🛠️ Tecnologias e Frameworks

* **Runtime:** .NET 8.0 (BackgroundService)
* **Mensageria:** Confluent Kafka Client (Event-Driven Consumer)
* **Comunicação HTTP:** HttpClient Factory (Integração síncrona com APIs de suporte)
* **Containers & Orquestração:** Docker & Kubernetes
---

## 🎯 Escopo e Funcionamento Interno

Diferente de uma API Web tradicional, este projeto não expõe endpoints REST. Ele atua como um Worker reativo baseado em filas e mensagens, executando o seguinte ciclo de vida arquitetural:

1. **Assinatura (`KafkaConsumerWorker`):** O serviço inicializa uma tarefa de segundo plano (`BackgroundService`) que conecta ao cluster Kafka e assina o tópico configurado.
2. **Consumo de Eventos (`PaymentProcessedEventConsumer`):** O Worker fica em escuta contínua aguardando mensagens no tópico de pagamentos processados (`PaymentProcessedCreatedEvent`).
3. **Mapeamento e Validação:** Ao receber uma mensagem válida e desserializá-la, o Worker recupera o identificador do pedido e os dados do comprador.
4. **Atualização do Pedido (`IOrderGameService`):** Faz uma chamada de integração para garantir a validação e atualização do estado do pedido com base nos dados do evento.
5. **Vínculo de Posse (`IBibliotecaUsuarioService`):** Registra de forma definitiva a relação entre o `IdUser` e o `IdGame` através do objeto `BibliotecaUsuario`, liberando o acesso ao jogo na plataforma.

---

### Repositório do Ecossistema
Você precisará clonar o seguintes repositório do projeto:

| Repositório | Link para Clone |
| :--- | :--- |
| **Catalog WS** | `https://github.com/rodrigosiqsilva/FIAP.PosTech.ArqSistemas.Catalog.git` |

### 🧪 Estratégia de Testes rápidos e Validação
Por se tratar de um serviço consumidor, a validação da sua saúde e o correto processamento de ponta a ponta dependem do envio de estímulos (mensagens) ao ecossistema:

Simulação de Eventos: Utilize uma ferramenta produtora de Kafka (como o AKHQ, Kafka CLI ou extensões do VS Code) para postar um payload JSON válido no tópico configurado (PaymentProcessed), simulando a aprovação de uma compra.

Rastreabilidade via Logs: O Worker possui interceptação de log estruturada. Monitore as saídas do console para rastrear o fluxo:

Mensagens de sucesso: "Biblioteca do usuário com Id {...} encontrada" ou "Jogo adicionado à biblioteca com sucesso".

Mensagens de inconsistência técnica ou falha de negócio: "Não foi encontrado registro do pedido..." ou "Não foi possível adicionar o jogo na biblioteca...".

### 📂 Estrutura de Pastas Obrigatória
Para que os arquivos de orquestração local (Docker Compose) referenciem os projetos corretamente, você **deve** respeitar a seguinte estrutura de diretórios no seu disco:

Veja um exemplo através da imagem: https://github.com/rodrigosiqsilva/FIAP.PosTech.ArqSistemas.Orchestrator/blob/main/Estrututa%20pastas.png

```text
C:\Sistemas\FIAP\     
├── FIAP.PosTech.ArqSistemas.Catalog/  <- (Arquivos desse repositório mencionados aqui)
├── FIAP.PosTech.ArqSistemas.User/
├── FIAP.PosTech.ArqSistemas.Notification/
└── FIAP.PosTech.ArqSistemas.Payments/