# InstaHashtagGrabber

Ermöglicht das Abrufen sämtlicher Beiträge zu einem Hashtag über die Instagram-API. Die Beiträge werden in antichronologischer Reihenfolge heruntergeladen und in eine MSSQL-Datenbank gespeichert. Zusätlich werden die wahrscheinlichsten Sprachen eines Beitrage mittels Spracherkennungs-Algorithmus TextCat ermittelt und ebenfalls in der Datenbank festgehalten.

Das festhalten der Beiträge in der Datenbank ermöglicht zu einem späteren Zeitpunkt umfangreiche Auswertung und Analysen.

## Konfiguration
Zuerst muss eine Konfigurations-Datei `config.json` in dem Hauptordner der Software erstellt werden. Diese beinhalten alle nötigen Informationen für den Zugriff auf die Instagram-API sowie den Connection-String zur SQL-Datenbank in der die Daten gespeichert werden.

```json
{
  "ConnectionString": "mssqlConnectionString",
  "InstagramUserName": "username",
  "InstagramPassword": "password",
  "InstagramCsrfToken": "instagramCsrfToken",
  "MediaMinDate": "2016-01-01T00:00:00"
}
```

## Daten abrufen
Beim Starten der Anwendung wird nach dem gewünschten Hashtag gefragt. Zu diesem werden dann sämtliche Beiträge in antichronologischer Reihenfolge heruntergeladen und in die Datenbank gespeichert.

## Speicherpunkt
Beim Abrufen der Daten wird immer wieder ein Speicherpunkt [savepoint] festgehalten. Mit diesem kann die Datenabfrage zu einem bestimmten Hashtag zu einem späteren Zeitpunkt fortgesetzt werden. Dazu muss beim Starten der Anwendung einfach der Hashtag erneut angegeben werden - die Frage ob der Speicherpunkt [savepoint] genutzt werden soll muss entsprechend mit Ja [y] beantwortet werden.