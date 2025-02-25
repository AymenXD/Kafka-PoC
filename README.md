# Kafka Proof of Concept

## Table of Contents
- [Project Architecture Overview](#project-architecture-overview)
- [Dockerized Services](#dockerized-services)
- [SSL Connection for Secure Kafka Communication](#ssl-connection-for-secure-kafka-communication)
- [Docker Environment Variables](#docker-environment-variables)
- [How to Use](#how-to-use)
- [Grafana and Prometheus for Metrics Visualization](#grafana-and-prometheus-for-metrics-visualization)
- [ELK Stack Integration](#elk-stack-integration)
- [Accessing Additional Tools](#accessing-additional-tools)


## Project Architecture Overview

This project is based on the Lambda Architecture, which is designed to handle massive quantities of data by taking advantage ofreal-time processing. The architecture consists of four primary layers

The diagram below illustrates this architecture. The Proof of Concept (PoC) will focus on the components highlighted within the red square, specifically the integration of data sources, Kafka, and MongoDB.

![architecture](/uploads/2acff8caf646d85656ad7cdb8813f61f/image.png){width=860 height=358}

The Proof of Concept (PoC) will demonstrate the data flow between various componernts. Producers generate data from different sources, such as ATS 1, ATS 2, ATS 3, and VdS, each sending their respective data to designated Kafka topics. Kafka then acts as the central message broker, allowing consumers to subscribe to these topics, process the incoming data, and store the results in MongoDB. 

![demo](/uploads/9d4934e0c27f67803ce88c59076a5cdf/image.png){width=840 height=457}

## Dockerized Services

The project includes Dockerized versions of the Producer (for both ATS1-2-3 and VdS clients), Consumer (ATS-Group, VdS-Group) services, as well as the Data-Generation service (to create random info for ATS1-2-3). This setup enables easy deployment and scalability.

The following diagram describes the connection between these different docker containers inside Docker Copmose:

![docker-system](/uploads/d94c58fb6f677b1905a748bd6a54896b/docker-system.png)

### Docker Images

- **VdS-Sample-Server**: Provides the VdS producer service, which connects to Kafka for data handling. The server is part of vdsstacklib project 
- **Producer Services**: This Producer service is for ATS1/2/3. It will fetch data from specific endpoints of the data-generation image and send it to Kafka.
- **Consumer Service**: This Consumer image will work for both VdS and ATS to consume messages from Kafka topics and stores the processed data in MongoDB.
- **Data-Generation Service**: Provides REST endpoints that simulate IoT data streams for ATS1/2/3.

### Docker Image Versions

There are two versions of the Producer and Consumer images available:

- **latest**: This version does not include SSL encryption, making it suitable for unsecured, internal testing environments.
- **ssl**: This version is configured with SSL encryption for secure communication between Kafka, producers, and consumers.

You can check these versions in http://dip.ad.atsonline.de/

## SSL Connection for Secure Kafka Communication

To secure communication between Kafka brokers, producers, and consumers, SSL encryption has been implemented. This ensures that data is transmitted securely, preventing unauthorized access.

#### Required Files
- **TLS_CA.crt**: Certificate authority certificate.
- **TLS_Server.pfx**: Server certificate file in PKCS #12 format.
- **TLS_Client.pfx**: Client certificate file in PKCS #12 format.

#### Server Configuration (for Kafka)

To configure SSL in Kafka, modify the following parameters in your Kafka properties file:

```properties
ssl.truststore.location=/path/to/TLS_CA.crt
ssl.keystore.location=/path/to/TLS_Server.pfx
ssl.keystore.password=your_keystore_password
```
#### Client Configuration (for .NET)

In .NET, you can configure the Kafka client to use SSL by modifying the ProducerConfig (or ConsumerConfig for consumers) as follows:

```csharp
var config = new ProducerConfig { 
   BootstrapServers = bootstrapServers,
   SecurityProtocol = SecurityProtocol.Ssl,
   SslCaLocation = caCertPath,
   SslKeystoreLocation = certPath,
   SslKeyPassword = certPassword,
   SslKeystorePassword = certPassword,
   SslEndpointIdentificationAlgorithm = SslEndpointIdentificationAlgorithm.None,
};
```

- BootstrapServers: Set to the Kafka bootstrap server addresses.
- SecurityProtocol: Set to Ssl to enable SSL encryption.
- SslCaLocation: Path to the CA certificate.
- SslKeystoreLocation: Path to the client certificate file.
- SslKeyPassword and SslKeystorePassword: Set to the password for the client certificate.

##### Docker Configuration for Certificates
To provide paths and passwords for certificates in Docker, you can use environment variables in docker-compose.yml to mount the certificates and set the paths accordingly. Example configuration:

```yaml
Producer / Consumer:
    environment:
      - Certificate__CAPath=/app/Certificates/TLS_CA.crt
      - Certificate__Path=/app/Certificates/TLS_Client.pfx
      - Certificate__Password=test123
    volumes:
      - ./path/to/certificates:/app/Certificates
```

- Certificate__CAPath: Path to the CA certificate inside the container.
- Certificate__Path: Path to the client certificate.
- Certificate__Password: Password for accessing the client certificate.

By mounting the certificates through Docker volumes and adjusting the paths in the Docker configuration, you ensure that SSL settings are correctly applied to secure communication within your Kafka setup.

## Docker Environment Variables

The behavior of the Dockerized services can be customized using environment variables:

### VdS-Sample-Server (Producer-VdS) Service Environment Variables

 Environment Variables
- **`Port_IPv1`**: Port for IPv1 communication (default: `50000`).
- **`Port_SecurIP`**: Port for secure IP communication (default: `60000`).
- **`Kafka__Brokers`**: Kafka broker addresses (default: `kafka-0:9092,kafka-1:9092`).
- **`Kafka__Topic`**: The Kafka topic for VdS data (default: `vds-data`).

### Producer-ATS Service Environment Variables

- **`Kafka__Brokers`**: The Kafka broker addresses (default: `kafka-0:9092,kafka-1:9092`).
- **`Kafka__Create_Topics__0`, `Kafka__Create_Topics__1`, `Kafka__Create_Topics__2`**: The list of topics that should be created if they don’t exist (`ats1-iot-data`, `ats2-iot-data`, `ats3-iot-data`) you can create as much topics as you want by following the format of `Kafka__Create_Topics__$nextNumber`.
- **`Companies__ATS1_API`**: The API endpoint for ATS1 data (default: `http://data-generation:5247/data/ats1`).
- **`Companies__ATS1_Topic`**: The Kafka topic for ATS1 data (default: `ats1-iot-data`).
- **`Companies__ATS1_Delay`**: The delay in milliseconds between each data generation for ATS1 (default: `1000`).
- **`Companies__ATS2_API`**: The API endpoint for ATS2 data (default: `http://data-generation:5247/data/ats2`).
- **`Companies__ATS2_Topic`**: The Kafka topic for ATS2 data (default: `ats2-iot-data`).
- **`Companies__ATS2_Delay`**: The delay in milliseconds between each data generation for ATS2 (default: `5000`).
- **`Companies__ATS3_API`**: The API endpoint for ATS3 data (default: `http://data-generation:5247/data/ats3`).
- **`Companies__ATS3_Topic`**: The Kafka topic for ATS3 data (default: `ats3-iot-data`).
- **`Companies__ATS3_Delay`**: The delay in milliseconds between each data generation for ATS3 (default: `10000`).

### Consumer Service Environment Variables

- **`Kafka__Brokers`**: The Kafka broker addresses (default: `kafka-0:9092,kafka-1:9092`).
- **`Kafka__Consumer_Group`**: The Kafka consumer group ID (default: `ats-group`).
- **`Kafka__Create_Topics__0`, `Kafka__Create_Topics__1`, `Kafka__Create_Topics__2`**: The list of topics that should be created if they don’t exist (`ats1-iot-data`, `ats2-iot-data`, `ats3-iot-data`) you can create as much topics as you want by following the format of `Kafka__Create_Topics__$nextNumber`.
- **`Kafka__Subscribe_Topics__0`, `Kafka__Subscribe_Topics__1`, `Kafka__Subscribe_Topics__2`**: The list of topics the consumer should subscribe to (`ats1-iot-data`, `ats2-iot-data`, `ats3-iot-data`) you can subscribe to as much topics as you want by following the format of `Kafka__Subscriber_Topics__$nextNumber`.
- **`ConnectionString`**: The MongoDB connection string for storing processed data (default: `mongodb://mongo0:27017,mongo1:27018,mongo2:27019/?replicaSet=rs0`).

## How to Use

First, you need to make sure that your Docker Desktop can connect to ATS Docker registry by adding the following to your Docker engine settings:
   ```sh
   "insecure-registries": [
      "dip.ad.atsonline.de:5000"
   ]
   ```

### Running the Project

To run the whole project, you just need to:

   ```sh
   docker-compose up -d
   ```

## Grafana and Prometheus for Metrics Visualization

This PoC also includes monitoring and visualization using Grafana and Prometheus. These tools help track key metrics from Kafka, the ASP.NET Core applications, and MSSQL.

1. **Prometheus** is configured to scrape metrics from Kafka, producers, and consumers, which are then stored for querying.
2. **Grafana** provides dashboards for visualizing these metrics.

To access Prometheus and see if the "Tragets" are working:
  - URL: `http://localhost:9090/targets`

To access Grafana:
  - URL: `http://localhost:3000`
  - Default login: `admin/admin`
  - Add a new connection to "prometheus" from the menu panel and connect to (prometheus:9090)
  - To import a pre-configured Kafka dashboard :
      - Download the provided JSON
         [Kafka-PoC_Dashboard.json](/uploads/a35d74485035163ff142e56c6fa9a89d/Kafka-PoC_Dashboard-1731005501157.json)
      - Open Grafana, go to "Dashboards" > "Import"
      - Upload or paste the JSON code.

## ELK Stack Integration

### What is the ELK Stack?

The **ELK Stack** (Elasticsearch, Logstash, and Kibana) is a powerful set of tools used for log aggregation, search, and visualization. It plays a crucial role in centralizing and analyzing logs from various sources, making it ideal for tracking application performance and troubleshooting issues. Integrating the ELK Stack in this project enables us to capture, process, and visualize log data from our Kafka-based system.

- **Elasticsearch** stores and indexes logs, allowing fast search and analysis.
- **Logstash** filters, processes, and forwards logs to Elasticsearch.
- **Kibana** provides an interface for querying and visualizing logs stored in Elasticsearch.

![Screenshot_2024-11-07_191957](/uploads/5e16ddb4aec7106790ce4a77fba3c44b/Screenshot_2024-11-07_191957.png)

### Setting Up Logstash

> **Note**: This setup step is optional as Logstash is already configured in this project. However, you may follow these instructions if you need to customize the Logstash configuration.


To set up Logstash for this project, Configure `logstash.conf`.

The `logstash.conf` file defines how Logstash processes logs and where it sends them. Below is a sample configuration for Logstash to collect Kafka logs and send them to Elasticsearch.

   ```plaintext
   input {
       tcp {
           port => 5044
           codec => json
       }
   }

   filter {
       # Add custom filters here to process logs as needed
   }

   output {
       elasticsearch {
           hosts => ["http://elasticsearch:9200"]
           index => "logs-%{+YYYY.MM.dd}"
       }
   }
   ```
   
- input: Configures Logstash to listen for logs on a TCP port, using the JSON codec.
- filter: Allows custom filters to modify or enrich logs.
- output: Sends the processed logs to Elasticsearch, organizing them by date.

### Running and Configuring Kibana 

Kibana allows you to visualize the logs stored in Elasticsearch. Follow these steps to set up Kibana:

1. **Add a Data Source**  
   Visit [http://localhost:8082/app/management/kibana/dataViews](http://localhost:8082/app/management/kibana/dataViews) to add the data source that stores logs in Elasticsearch. Click on **Create Data View**.

   ![Add-View](/uploads/36d9798af0049a53b5abc67f92192655/Screenshot_2024-11-08_012032.png)

2. **Configure the Index Pattern**  
   - Enter a name for the data view.
   - Set the index pattern to **logs-***, which should match the naming convention specified in the `output` section of `logstash.conf`. This pattern allows Kibana to recognize all indices with names starting with "logs-", ensuring all related log data is included in your view.
   - In the right section of the screenshot, Kibana will display the detected index patterns and highlights it.

   ![Add-Index-Pattern](/uploads/931fbf9699ea8a569cab35781e720e3a/Screenshot_2024-11-08_012331.png)

3. **Explore Logs in Discover**  
   Go to [http://localhost:8082/app/discover](http://localhost:8082/app/discover) to view your logs. Select your data view, and from the left menu, you can add or remove fields as columns to customize the displayed information. This allows you to focus on specific log details relevant to your analysis.

![Dashboard](/uploads/91eac5dd85bcb50916470ed2143e6771/Screenshot_2024-11-07_191957.png)

## Accessing Additional Tools

- **AKHQ**: Provides access to manage and monitor Kafka clusters using the following credentials:
   - URL: http://localhost:8080
- **MongoDB Express**: Access MongoDB using the following credentials:
   - URL: http://localhost:8081
   - Username: admin
   - Password: pass