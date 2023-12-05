CREATE TABLE account (
  id int(11) NOT NULL AUTO_INCREMENT,
  admin tinyint(4) NOT NULL DEFAULT 0,
  username varchar(255) NOT NULL,
  email varchar(255) NOT NULL,
  password varchar(255) NOT NULL,
  active tinyint(4) NOT NULL DEFAULT 0,
  activation_expiry datetime NOT NULL,
  activation_code varchar(255) DEFAULT NULL,
  activated_dt datetime DEFAULT NULL,
  reset_expiry datetime DEFAULT NULL,
  reset_code varchar(255) DEFAULT NULL,
  created_dt datetime DEFAULT current_timestamp(),
  updated_dt datetime NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (id),
  UNIQUE KEY email (email),
  KEY username (username)
);

CREATE TABLE device (
  id int(11) NOT NULL AUTO_INCREMENT,
  fk_account_id int(11) NOT NULL,
  device_guid varchar(50) NOT NULL,
  device_type varchar(20) NOT NULL,
  machine_name varchar(255) NOT NULL,
  forget_expiry datetime DEFAULT NULL,
  forget_code varchar(255) DEFAULT NULL,  
  active tinyint(4) NOT NULL DEFAULT 0,
  activation_code varchar(255) DEFAULT NULL,
  activated_dt datetime DEFAULT NULL,
  detail1 varchar(50) NOT NULL,
  detail2 varchar(50) NOT NULL,
  detail3 varchar(50) NOT NULL,
  created_dt datetime DEFAULT current_timestamp(),
  updated_dt datetime NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (id),
  UNIQUE KEY device_guid (device_guid),
  FOREIGN KEY (fk_account_id) REFERENCES account(id)
);

CREATE TABLE subscription (
  id int(11) NOT NULL AUTO_INCREMENT,
  device_guid varchar(50) NOT NULL,
  fk_account_id int(11) DEFAULT NULL,
  sub_type varchar(30) NOT NULL,
  monthly tinyint(4) NOT NULL DEFAULT 0,
  expires_utc_dt datetime DEFAULT NULL,
  created_dt datetime DEFAULT current_timestamp(),
  updated_dt datetime NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (id),
  UNIQUE KEY device_guid (device_guid),
  FOREIGN KEY (fk_account_id) REFERENCES account(id)
);