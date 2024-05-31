CREATE DATABASE IF NOT EXISTS campusecorival;

USE campusecorival;

CREATE TABLE IF NOT EXISTS data (
  id INT AUTO_INCREMENT PRIMARY KEY,
  school VARCHAR(255) NOT NULL,
  room VARCHAR(255),
  dataType VARCHAR(50) NOT NULL,
  dataValue FLOAT NOT NULL,
  date VARCHAR(50) NOT NULL,
  country VARCHAR(50) NOT NULL,
  postcode VARCHAR(50),
  state VARCHAR(50)
);