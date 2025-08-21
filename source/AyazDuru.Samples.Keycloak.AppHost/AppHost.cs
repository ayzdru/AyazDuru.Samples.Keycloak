var builder = DistributedApplication.CreateBuilder(args);


//Blog yazýsýndaki adýmlarý hýzlýca eklemek isterseniz. aþaðýdaki yapýlandýrma dosyasýný kullanabilirsiniz.
//.WithRealmImport("e-commerce-realm-export.json");
var keycloak = builder.AddKeycloak("keycloak", 8080).WithImageTag("26.3.3").WithDataVolume("keycloak_data");
 
var rabbitmq = builder.AddRabbitMQ("rabbitmq").WithImageTag("3-management").WithDataVolume("rabbitmq_data");

var productApiService = builder.AddProject<Projects.AyazDuru_Samples_Keycloak_ProductApiService>("productapiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(keycloak)
    .WithReference(rabbitmq)
    .WaitFor(keycloak)
    .WaitFor(rabbitmq);

var notificationApiService = builder.AddProject<Projects.AyazDuru_Samples_Keycloak_NotificationApiService>("notificationapiservice")
    .WithHttpHealthCheck("/health").WaitFor(keycloak)
    .WaitFor(rabbitmq);



builder.AddProject<Projects.AyazDuru_Samples_Keycloak_Blazor>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")    
    .WithReference(productApiService)
    .WaitFor(productApiService)
    .WithReference(notificationApiService)
    .WaitFor(notificationApiService);

builder.Build().Run();
