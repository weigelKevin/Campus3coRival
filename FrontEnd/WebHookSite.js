document.addEventListener('DOMContentLoaded', function() {
    const dateInput = document.getElementById('date');
    const today = new Date().toLocaleDateString('de-DE');
    dateInput.value = today;
});

document.getElementById('dataType').addEventListener('change', function(event) {
    const dataInput = document.getElementById('dataInput');
    const dataLabel = document.getElementById('dataLabel');

    if (event.target.value === 'electricity') {
        dataLabel.textContent = 'Stromverbrauch (kWh):';
        dataInput.classList.remove('hidden');
    } else if (event.target.value === 'water') {
        dataLabel.textContent = 'Wasserverbrauch (Liter):';
        dataInput.classList.remove('hidden');
    } else {
        dataInput.classList.add('hidden');
    }
});

function toggleGermanyFields() {
    const germanyFields = document.getElementById('germanyFields');
    const countrySelect = document.getElementById('country');
    
    if (countrySelect.value === 'Germany') {
        germanyFields.classList.remove('hidden');
    } else {
        germanyFields.classList.add('hidden');
    }
}

document.getElementById('sustainabilityForm').addEventListener('submit', function(event) {
    event.preventDefault();

    const data = {
        school: document.getElementById('school').value,
        room: document.getElementById('room').value || null,
        dataType: document.getElementById('dataType').value,
        dataValue: document.getElementById('dataValue').value || null,
        date: document.getElementById('date').value,
        country: document.getElementById('country').value,
        postcode: document.getElementById('postcode').value || null,
        state: document.getElementById('state').value || null
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
            return response.text().then(text => {
                try {
                    const errorData = JSON.parse(text);
                    throw new Error('Error ' + response.status + ': ' + (errorData.message || errorData.error || text));
                } catch (err) {
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
