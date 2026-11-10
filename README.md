# BASF-Print-Dashboard

Lokaler Basisstand für das Druckerflotten-Dashboard. Die Demo zeigt fünf synthetische Geräte mit Flottenkennzahlen, kombinierbaren Inventarfiltern und Gerätedetails. Der Bestand wird sitzungsübergreifend im Speicher gehalten. Die Übersicht aktualisiert sich im konfigurierten Intervall und per Schaltfläche.

## Lokal starten

Voraussetzung ist das .NET 9 SDK. Der Basisstand verwendet `net9.0` für die Kompatibilität mit der vorhandenen Visual-Studio-Version. In der Spezifikation ist .NET 10 als Ziel vorgesehen.

Im Projektordner ausführen.

```powershell
dotnet run --project src/BASF.Print.Monitor.Web --launch-profile Demo
```

Die Demo ist unter [localhost:5180](http://localhost:5180) erreichbar. Sie startet im Development-Profil mit aktiviertem Mock und akzeptiert ausschließlich lokale Verbindungen.

## Validieren

```powershell
dotnet test BASF.Print.Monitor.sln
```

Der Basisstand verwendet vorbereitete Demo-Statuswerte und macht keine Geräte- oder API-Abfragen. Zabbix-Anbindung, Produktionsauthentifizierung, Betriebsdiagnostik und der reale Geräte-PoC gehören zu den folgenden Umsetzungsschritten. Die Spezifikationen liegen im übergeordneten Arbeitsverzeichnis unter `Doku/Neu`.
