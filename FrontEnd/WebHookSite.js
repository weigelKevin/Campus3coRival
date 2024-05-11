document.getElementById('sustainabilityForm').addEventListener('submit', function(event) {
    event.preventDefault();

    // Daten sammeln
    const data = {
        tenantId: document.getElementById('tenantId').value,
        school: document.getElementById('school').value,
        room: document.getElementById('room').value || null,
        electricity: document.getElementById('electricity').value || null,
        water: document.getElementById('water').value || null
    };

    // URL Ihrer Azure Function
    const azureFunctionUrl = 'https://campusecorivaldatareceiver.azurewebsites.net/api/WebhookDataReceiver?code=LBhH6nwgUFJ-RedsB1thlnHQyjsrQMU6Ia0ie5hUHm6aAzFuzJSAXg==';

    fetch(azureFunctionUrl, {
        method: 'POST',
        body: JSON.stringify(data),
        headers: {
            'Content-Type': 'application/json'
        }
    })
    .then(response => {
        // Überprüfen, ob der HTTP-Statuscode erfolgreich ist
        if (!response.ok) {
            // Lesen Sie den Body als Text, um sowohl Text- als auch JSON-Fehlermeldungen zu behandeln
            return response.text().then(text => {
                try {
                    // Versuchen, den Text als JSON zu interpretieren
                    const errorData = JSON.parse(text);
                    throw new Error('Error ' + response.status + ': ' + (errorData.message || errorData.error || text));
                } catch (err) {
                    // Wenn es kein JSON ist, verwenden Sie den reinen Text
                    throw new Error('Error ' + response.status + ': ' + text);
                }
            });
        }
        return response.json();
    })
    .then(data => {
        console.log('Success:', data);
        alert('Daten erfolgreich gesendet!');
    })
    .catch(error => {
        console.error('Error:', error);
        alert('Fehler beim Senden der Daten: ' + error.message);
    });
});
