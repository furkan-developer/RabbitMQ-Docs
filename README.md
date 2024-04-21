# Overview
 - https://www.milanjovanovic.tech/blog/using-masstransit-with-rabbitmq-and-azure-service-bus?utm_source=newsletter&utm_medium=email&utm_campaign=tnw78

# Base configuration
> MassTransit is usable in most .NET application types. MassTransit is easily configured in ASP.NET Core or .NET Generic Host applications (using .NET 6 or later). [Configuration](https://masstransit.io/documentation/configuration#configuration)


**Requirements package**

- dotnet add package Masstransit.AspNetCore (this is Because of Will use AspNet Web API for examples)
- dotnet add package Masstransit.RabbitMQ


```csharp
// Program.cs
builder.ConfigureServices(service =>
{
    service.AddMassTransit((config) =>
    {
        config.UsingRabbitMq((ctx, cfg) =>
        {
            cfg.Host(host: "amqps://welaavja:x6hkeE8trOMn48bGFhmy_q1hgpCYlZA-@crow.rmq.cloudamqp.com/welaavja");

            // ConfigureEndpoints should be the last method called after all settings and middleware components have been configured.
            cfg.ConfigureEndpoints(ctx);
        });
    });
});
```

Yukarıdaki koddan tahmin edileceği gibi projenin servis koleksiyonuna masstrasit paketi tarafından oluşturulan servis ekleniyor. Bu yapılandırma her bir procuder ve consumer projelerinde temel yapılandırma ayarıdır.

Procuder ve Consumer projelerinde Program.cs dosyasına bakıldığında yukarıdaki konfigürasyonel ayarların mevcut olduğu görülebilir.

# Messages
> In MassTransit, a message contract is defined code first by creating a .NET type. A message can be defined using a record, class, or interface. Messages should only consist of **properties**, methods and other behavior should not be included. [Message](https://masstransit.io/documentation/concepts/messages#messages)

```
MassTransit uses the full type name, including the namespace, for message contracts. When creating the same message type in two separate projects, the namespaces must match or the message will not be consumed.
```

Masstansit sistemler arası veri paylaşımını sağlamak için kullanılan mesaj kontratının aynı namespace altında olmasını ister. Eğer iki sistem arasında veri paylaşımı yapılır iken kullanılan mesaj kontratının namespace leri tam olarak uyuşmaz ise veri paylaşımı istenildiği gibi yapılamaz. Masstransit tarafından istenilen bu kısıtlamayı dokümanın devamında tekrar değinilecektir.

İki farklı projec arasında veri paylaşımını sağlayabilmek için bir class library oluşturuşup burada mesaj kontratları tanımlanabilir. Repoda Shared projesi altında mesaj kontratları tanımlanmıştır.

```
Messages must be reference types, and can be defined using **records**, **interfaces**, or **classes**. [Message Contract Types](https://masstransit.io/documentation/concepts/messages#message-types)
```

# Producer
```csharp
// Producer project
public class ItemController : ControllerBase
{
    private readonly IPublishEndpoint _publish;

    public ItemController(IPublishEndpoint publish)
    {
        _publish = publish;
    }

    [HttpPost]
    public IActionResult CreateItem()
    {
        // Add code for creating a new item

        // Publish message for item created to rabbitMQ server
        _publish.Publish(message: new ItemCreatedMessage(Name: "Item-1"));
        System.Console.WriteLine("Published Item-1");
        
        return Created();
    }
}
```
Projede **ItemCreatedMessage** referansı bir mesaj kontratı olarak kullanıldığından Publish metodu ile iligli rabbitMQ sunucusuna gönderiliyor. 

# Consumer
Bir projede herhangi bir message kontratı tüketilecek ise **IConsumer<T>** interface ile implemente edilmiş bir sınıf oluşturulmalıdır. Aşağıda **ItemCreatedMessage** mesaj kontratını tüketecek consumer sınıfı oluşturulmuştur. 

```csharp
// Consumer project
public class ItemCreatedMessageConsume : IConsumer<ItemCreatedMessage>
{
    public Task Consume(ConsumeContext<ItemCreatedMessage> context)
    {
        ItemCreatedMessage message = context.Message;
        
        System.Console.WriteLine($"Recieved message details: Item Name: {message.Name}");

        return Task.CompletedTask;
    }
}
```

Daha sonra oluşturulan consumer sınıfının aşağıdaki **AddConsumer()** metodu ile kaydı yapılmalıdır. Ayrıca ilgili consumer için **recieve endpoint** ayarlarınında yapılması gerekir. Şimdilik **ConfigureEndpoints()** metodu ile default recieve endpoint kaydı yapılıyor. (Consumer Registration)[https://masstransit.io/documentation/configuration#consumer-registration]

```csharp
service.AddMassTransit((config) =>
{
    config.AddConsumer<ItemCreatedMessageConsume>();

    config.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(host: "amqps://welaavja:x6hkeE8trOMn48bGFhmy_q1hgpCYlZA-@crow.rmq.cloudamqp.com/welaavja");

        // ConfigureEndpoints should be the last method called after all settings and middleware components have been configured.
        cfg.ConfigureEndpoints(ctx);
    });
});
```

# Run
Buraya kadar anlatılanlar ile bir çıktı alabilmek için Procuder ve Consumer projeleri run edilebilir. 

**Not:** Örneklerde (CloudAMQP)[https://www.cloudamqp.com/] sitesinden RabbitMQ hizmeti kullanıldığı için ilgili host bilgileri kullanılmıştır. Kullanacağınız RabbitMQ server sunucusunun bilgileri ile projelerin ilgili yerlerini güncellemeyi unutmayınız.

Consumer projesi run edildiğinde RabbitMQ Manager panelden oluşan exchange lerin ve queue nun ismi aşağıdaki gibi olacaktır.

![](./Docs/exchanges.png)
![](./Docs/queue.png)

Yukarıdaki görsellere bakıldığında oluşan exchange ler, **ItemCreatedMessageConsume** ve **Shared.Messages:ItemCreatedMessage** dır. Ayrıca oluşan kuyruğun ismide **ItemCreatedMessageConsume** olarak isimlendirilmiştir.

Özetle bir consumer projesi için default konfigürasyonel ayarlar kullanılır ise; kullanılan mesaj tipi ile aynı isme sahip bir exchange ve IConsumer<> dan implemente edilen sınıf ile aynı isme sahip bir exchange ve queue oluşturulur. 

Oluşturulan queue ya mesaj yollamak için Procuder projesine bir request atıldığında Consumer projesinin ilgili mesajı tükettiği aşağıdaki gibi görüllebilir. 

![](./Docs/output-1.png)

Buraya kadar her şey güzel. Peki aynı konfigürasyonel ayarlar ve aynı mesaj tipi ile bir Consumer projesi daha ayağa kaldırıldığında nasıl bir davranış sergilenecektir? Ikinci bir consumer projesi ayağa kaldırmak için mevcut Consumer projesinin bir kopyasını oluşturup Procuder projesine iki kez request atıldığındaki aşağıdaki gibi çıktılar oluşacaktır.

![](./Docs/output-2.png)

Yukarıdaki görsele bakıldığında Procuder iki kez mesaj yayınlıyor ama iki consumer da sadece bir mesaj tüketiyor. Çünkü ayağa kaldırılan ikinci Consumer projeside Masstransit için aynı konfigürasyonel ayarları ve mesaj tipini kullandığı için daha önceden bahsettiğimiz ItemCreatedMessageConsume kuyruğunu tüketiyor. Şuan kuyruğu tüketen mevcut iki consumer olduğundan kuyruk sırasıyla consumer'lara mesajları iletir.

# Configure Custom Enpoint
Endpoint ayarlarını manuel bir şekilde yapabilmek için **RecieveEndpoint()** fonksiyonu kullanılır.

Hadi bir tane daha Consumer projesinin kopyasını oluşturup aşağıdaki kod ile endpoint yapılandırılmasını değiştirelim. Kod incelendiğinde ReciveEndpoint fonksiyonuna il argüman olarak bir kuyruk ismi veriliyor. 

```csharp
service.AddMassTransit((config) =>
{
    config.AddConsumer<ExampleConsume>();

    config.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(host: "amqps://welaavja:x6hkeE8trOMn48bGFhmy_q1hgpCYlZA-@crow.rmq.cloudamqp.com/welaavja");

        // Manually configured receive endpoints should be configured before calling ConfigureEndpoints.
        cfg.ReceiveEndpoint(queueName: "specify-queue", e =>
        {
            e.ConfigureConsumer<ExampleConsume>(ctx);
        });

        // ConfigureEndpoints should be the last method called after all settings and middleware components have been configured.
        cfg.ConfigureEndpoints(ctx);
    });
});
```

Bu Consumer projesi ile birlikte mevcut 3 consumer projemiz oldu. Procuder projesi ile beraber tüm Consumer projelerinide ayağa kaldırıp tekar mesaj yayınlamayı deneyelim.

![](./Docs/output-3.png)

Yukarıdaki görsele bakıldığında Procuder projesinden tekarar iki mesaj yayınlandığında Consumer projelerinde kaç tane mesaj tüketildiği görülüyor. Consumer-3 projesi 2 mesaj tüketir iken diğer iki consumer projeleri bir mesaj tüketiyor. Çünkü Consumer-3 projesi için yapılan endpoint yapılandırma ayarlarında IConsumer<> dan implemente edilen sınıf için **specify-queue** adında bir kuyruğun oluşturulması söylendi. Daha önceden bahsettiğimiz gibi default olarak masstransit her kuyruk için bir exchange oluşturacağından dolayı specify-queue adında bir exchange daha oluşturuldu ve bu exchange e specify-queue kuyruğunu bind etti. Aşağıdaki şemada oluşan tüm exchange ler ve bunlara bind edilen kuyruklar gösteriliyor.

![](./Docs/scheme.png)

# Send Message

# Request/Reply pattern