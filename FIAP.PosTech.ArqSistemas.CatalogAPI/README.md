# FIAP.PosTech.ArqSistemas.CatalogAPI

API Web desenvolvida em **.NET 8** utilizando os princípios de **Domain-Driven Design (DDD)** 
e arquitetura de microsserviços. A aplicação expõe endpoints REST para operações síncronas e 
integra-se de forma assíncrona ao **Apache Kafka** para mensageria e eventos.

Ela é o componente central de gerenciamento de e-commerce e catálogo para uma plataforma de jogos 
digitais, a aplicação foi projetada seguindo práticas modernas de arquitetura de software para garantir 
escalabilidade, separação de responsabilidades (através de Camadas de Serviços e Controladores) e facilidade
de integração.

Essa API atua como o motor por trás da experiência do usuário, gerenciando desde a navegação em um catálogo 
dinâmico de jogos até o processamento interno de pedidos de compra e a disponibilização imediata dos 
títulos adquiridos na biblioteca particular de cada jogador.

---

## 🛠️ Tecnologias e Frameworks

* **Runtime:** .NET 8.0 SDK
* **Documentação:** Swagger / OpenAPI 3
* **Mensageria:** Confluent Kafka Client (Event-Driven)
* **Containers:** Docker & Kubernetes

---

## 🚀 Como Executar e Acessar o Swagger

### Pré-requisitos
* [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download) instalado.
* Infraestrutura de mensageria (Kafka) rodando localmente ou via container.

---

### Repositório do Ecossistema
Você precisará clonar o seguintes repositório do projeto:

| Repositório | Link para Clone |
| :--- | :--- |
| **Catalog API** | `https://github.com/rodrigosiqsilva/FIAP.PosTech.ArqSistemas.Catalog.git` |

### Testes

Para executar testes utilize o arquivo FIAP.PosTech.ArqSistemas.CatalogAPI.http presente na raiz do projeto, ele contém requisições pré-configuradas para testar os endpoints da API utilizando o recurso de HTTP Client do Visual Studio Code.

### 📂 Estrutura de Pastas Obrigatória
Para que os arquivos de orquestração local (Docker Compose) referenciem os projetos corretamente, você **deve** respeitar a seguinte estrutura de diretórios no seu disco:

Veja um exemplo através da imagem: https://github.com/rodrigosiqsilva/FIAP.PosTech.ArqSistemas.Orchestrator/blob/main/Estrututa%20pastas.png

```text
C:\Sistemas\FIAP\     
├── FIAP.PosTech.ArqSistemas.Catalog/  <- (Arquivos desse repositório mencionados aqui)
├── FIAP.PosTech.ArqSistemas.User/
├── FIAP.PosTech.ArqSistemas.Notification/
└── FIAP.PosTech.ArqSistemas.Payments/