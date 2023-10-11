CREATE TABLE account (
    id INT NOT NULL PRIMARY KEY AUTO_INCREMENT,
    email VARCHAR(255) NOT NULL UNIQUE,
    username VARCHAR(50) NOT NULL UNIQUE,
    password VARCHAR(255) NOT NULL,
    created_dt DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE subscription (
    id INT NOT NULL PRIMARY KEY AUTO_INCREMENT,
    fk_account_id INT NOT NULL,
    device_guid VARCHAR(50) NOT NULL UNIQUE,
    sub_type VARCHAR(10) NOT NULL,
    expires_utc_dt DATETIME,    
    FOREIGN KEY (fk_account_id) REFERENCES account(id)
);