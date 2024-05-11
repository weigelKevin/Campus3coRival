document.getElementById('sustainabilityForm').addEventListener('submit', function(event) {
    event.preventDefault(); // Verhindert das normale Verhalten des Formular-Submits

    // Daten sammeln, die vom Formular gesendet werden
    const data = {
        tenantId: document.getElementById('tenantId').value, // Wert des Tenant ID Feldes
        school: document.getElementById('school').value,
        room: document.getElementById('room').value || null, // Optional, standardmäßig null wenn leer
        electricity: document.getElementById('electricity').value || null, // Optional
        water: document.getElementById('water').value || null // Optional
    };

    // URL deiner Azure Function
    const azureFunctionUrl = 'https://campusecorivaldatareceiver.azurewebsites.net/api/WebhookDataReceiver?code=LBhH6nwgUFJ-RedsB1thlnHQyjsrQMU6Ia0ie5hUHm6aAzFuzJSAXg==';

    // Anfrage an die Azure Function senden
    fetch(azureFunctionUrl, {
        method: 'POST', // HTTP-Methode
        body: JSON.stringify(data), // Daten als JSON-String
        headers: {
            'Content-Type': 'application/json' // Setzt den Inhaltstyp auf JSON
        }
    })
    .then(response => {
        if (!response.ok) { // Überprüft, ob der HTTP-Statuscode erfolgreich ist
            throw new Error('Network response was not ok ' + response.status);
        }
        return response.json(); // Verarbeitet die Antwort als JSON
    })
    .then(data => {
        console.log('Success:', data);
        alert('Daten erfolgreich gesendet!'); // Benachrichtigung über erfolgreichen Sendevorgang
    })
    .catch(error => {
        console.error('Error:', error);
        alert('Fehler beim Senden der Daten: ' + error.message); // Fehlermeldung, wenn ein Fehler auftritt
    });
});
