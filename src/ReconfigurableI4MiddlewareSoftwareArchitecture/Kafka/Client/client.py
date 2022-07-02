from kafka import KafkaProducer
producer = KafkaProducer(bootstrap_servers='192.168.1.12:9092')
topic = ""
message = ""
#producer.send(topic, str.encode(' {  "@id":"33bec44f-30d5-4716-af94-4c8f420bbba9",  "@type":"operation",  "aasOriginId":"i4.sdu.dk/Middleware/Orchestrator",  "aasTargetId":"i4.sdu.dk/Assembly/UR",  "orderId":"123456789",  "operation":"start",  "parameters":    {      "cellNumber":1     }   }'))
topic = "Transport"

# for i in range(100):
#      producer.send(topic, str.encode(' {"@id":"5d4099b6-ce00-4c1a-8c15-386408a61c64", "@type":"operation",  "aasOriginId":"i4.sdu.dk/Middleware/Orchestrator",  "aasTargetId":"i4.sdu.dk/Transport/Track",  "orderId":"123456789",  "operation":"moveAssemblyPart"   }'))

#topic = input("topic\n")
mes = '{  "@id":"2f0d7927-84ec-4334-b4ad-29b1d657ae2a",  "@type":"operation",  "aasOriginId":"i4.sdu.dk/Middleware/Orchestrator",  "aasTargetId":"i4.sdu.dk/Middleware/Orchestrator",  "orderId":"123456789",  "operation":"fabricateDrone", "parameters":{"recipe":"Cell1RightCell2Left", "orderId":"97a6weas4swewe1", "amount":10}  }'

for i in range(5):
    message1 = '{  "@id":"2f0d7927-84ec-4334-b4ad-29b1d657ae2a",  "@type":"operation",  "aasOriginId":"i4.sdu.dk/Middleware/Orchestrator",  "aasTargetId":"i4.sdu.dk/Middleware/Orchestrator",  "orderId":"123456789",  "operation":"fabricateDrone", "parameters":{"recipe":"Cell1RightCell2Left", "orderId":"orderRight' + str(i) + '", "amount":2}  }'
    message2 = '{  "@id":"2f0d7927-84ec-4334-b4ad-29b1d657ae2a",  "@type":"operation",  "aasOriginId":"i4.sdu.dk/Middleware/Orchestrator",  "aasTargetId":"i4.sdu.dk/Middleware/Orchestrator",  "orderId":"123456789",  "operation":"fabricateDrone", "parameters":{"recipe":"Cell1LeftCell2Right", "orderId":"orderLeft' + str(i) + '", "amount":2}  }'
    producer.send("Executions", str.encode(message1))
    producer.send("Executions", str.encode(message2)) 


while True:
    topic = input("topic\n")
    message = input("enter input\n")
    producer.send(topic, str.encode(message))

# Assembly
# {  "@id":"33bec44f-30d5-4716-af94-4c8f420bbba9",  "@type":"operation",  "aasOriginId":"i4.sdu.dk/Middleware/Orchestrator",  "aasTargetId":"i4.sdu.dk/Assembly/UR",  "orderId":"123456789",  "operation":"start",  "parameters":    {      "cellNumber":1     }   }
#{  "@id":"33bec44f-30d5-4716-af94-4c8f420bbba9",  "@type":"configuration_request",  "aasOriginId":"i4.sdu.dk/Middleware/Orchestrator",  "aasTargetId":"i4.sdu.dk/Middleware/Configurator",  "orderId":"123456789", "parameters": ["item1", "item2", "item3"]   }
# {"test":"test"}


# Starting on new order: "Order with id: orderLeft9
# 947120d1-4ead-4c91-b032-b99cdc3b369c