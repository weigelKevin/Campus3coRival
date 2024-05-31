const express = require('express');
const bodyParser = require('body-parser');
const mysql = require('mysql');
const cors = require('cors');

const app = express();
const port = 4000;

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

app.get('/data', (req, res) => {
  const query = 'SELECT * FROM data';
  db.query(query, (err, results) => {
    if (err) {
      console.error('Error fetching data:', err);
      res.status(500).json({ error: 'Error fetching data' });
      return;
    }
    res.status(200).json(results);
  });
});

app.listen(port, () => {
  console.log(`API server running at http://localhost:${port}`);
});
