document.getElementById('sustainabilityForm').addEventListener('submit', function(event) {
    event.preventDefault();

    const data = {
        school: document.getElementById('school').value,
        room: document.getElementById('room').value || null, // Optionaler Wert
        electricity: document.getElementById('electricity').value || null, // Optionaler Wert
        water: document.getElementById('water').value || null // Optionaler Wert
    };

    const azureFunctionUrl = 'https://campusecorivaldatareceiver.azurewebsites.net/api/WebhookDataReceiver?code=LBhH6nwgUFJ-RedsB1thlnHQyjsrQMU6Ia0ie5hUHm6aAzFuzJSAXg==';

    fetch(azureFunctionUrl, {
        method: 'POST',
        body: JSON.stringify(data),
        headers: {
            'Content-Type': 'application/json'
        }
    })
    .then(response => {
        if (!response.ok) {
            throw new Error('Network response was not ok');
        }
        return response.json();
    })
    .then(data => {
        console.log('Success:', data);
        alert('Daten erfolgreich gesendet!');
    })
    .catch(error => {
        console.error('Error:', error);
        if (error instanceof TypeError) {
            alert('Es gab ein Problem mit der Netzwerkverbindung.');
        } else {
            alert('Es gab ein unerwartetes Problem beim Senden der Daten.');
        }
    });
});
