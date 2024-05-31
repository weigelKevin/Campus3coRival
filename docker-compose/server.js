const express = require('express');
const bodyParser = require('body-parser');
const mysql = require('mysql');
const cors = require('cors');

const app = express();
const port = 3000;

const db = mysql.createConnection({
  host: 'db',
  user: 'user',
  password: 'password',
  database: 'campusecorival'
});

db.connect(err => {
  if (err) {
    console.error('Error connecting to the database:', err);
    return;
  }
  console.log('Connected to the database');
});

app.use(cors());
app.use(bodyParser.json());

app.post('/webhook', (req, res) => {
  const { school, room, dataType, dataValue, date, country, postcode, state } = req.body;
  const query = 'INSERT INTO data (school, room, dataType, dataValue, date, country, postcode, state) VALUES (?, ?, ?, ?, ?, ?, ?, ?)';
  const values = [school, room, dataType, dataValue, date, country, postcode, state];

  db.query(query, values, (err, result) => {
    if (err) {
      console.error('Error inserting data:', err);
      res.status(500).json({ error: 'Error inserting data' });
      return;
    }
    res.status(200).json({ message: 'Data received' });
  });
});

app.listen(port, () => {
  console.log(`Server running at http://localhost:${port}`);
});
