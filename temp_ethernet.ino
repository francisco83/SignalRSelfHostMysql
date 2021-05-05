/* CONTROL DE TEMPERATURA - HUMEDAD- VOLTAJE PANEL SOLAR 
*/

//Libraries
#include <DHT.h>
#include <SPI.h>
#include <Ethernet.h>

//Constants
#define DHTPIN 7     // what pin we're connected to
#define DHTTYPE DHT22   // DHT 22  (AM2302)
DHT dht(DHTPIN, DHTTYPE); //// Initialize DHT sensor for normal 16mhz Arduino

byte mac[] = { 0xDE, 0xAD, 0xBE, 0xEF, 0xFE, 0xED };
IPAddress ip(192, 168, 0, 40);
EthernetServer server(23);


//Variables
int chk;
float hum;  //Stores humidity value
float temp; //Stores temperature value
String stemp;
String shum;
float volt;

//sensor luz
const long A = 1000;     //Resistencia en oscuridad en KΩ
const int B = 15;        //Resistencia a la luz (10 Lux) en KΩ
const int Rc = 10;       //Resistencia calibracion en KΩ
const int LDRPin = A0;   //Pin del LDR
const int VOLTPin = A1;   //Pin del panel solar

int V;
int ilum;
String silum;
String svolt;

void setup()
{
  Serial.begin(9600);
  dht.begin();

  Ethernet.begin(mac, ip);
  server.begin();
  Serial.print("Servidor en: ");
  Serial.println(Ethernet.localIP());

}
  
void loop()
{
    
  EthernetClient client = server.available(); 
  if (client)
  {
    bool currentLineIsBlank = true;
     
    while (client.connected()) 
    {
      if (client.available()) 
      {

          hum = dht.readHumidity();
          temp= dht.readTemperature();

          V = analogRead(LDRPin);     
          volt = analogRead(VOLTPin);  
          //volt = volt * 4.16;//
          volt = volt * (5.0/1023.0)* 4;  
          ilum = ((long)V*A*10)/((long)B*Rc*(1024-V));    //usar si LDR entre A0 y Vcc (como en el esquema anterior)
  
          stemp = temp;   
          shum = hum;      
          silum = ilum;
          svolt = volt;
          client.print(stemp+" "+shum+" "+silum+" "+svolt);
          delay(5000);
          stemp="";
           
        /*
        char c = client.read();
        Serial.write(c);
        
        // Al recibir linea en blanco, servir página a cliente
        if (c == '\n' && currentLineIsBlank)
        {
          Serial.print("Ingreso a la pagina");
          client.println(F("HTTP/1.1 200 OK\nContent-Type: text/html"));
          client.println();
 
          client.println(F("<html>\n<head>\n<title>Panel de Control</title>\n</head>\n<body>"));
          client.println(F("<div style='text-align:center;'>"));  
          client.println("Temperatura");
          client.println(temp);
          client.println("Humedad"); 
          client.println(hum); 
          client.println("</body></html>");   
          break;   
        }
        if (c == '\n') 
        {
          Serial.print("Puso variable en true");
          currentLineIsBlank = true;
        }
        else if (c != '\r') 
        {
          Serial.print("Puso variable en false");
          currentLineIsBlank = false;
        }
        */
      }
    }  
    delay(1);
    client.stop();     
    Serial.println("cliente desconectedo");
  }
   
}
