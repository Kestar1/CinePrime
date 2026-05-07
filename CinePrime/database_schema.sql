CREATE DATABASE IF NOT EXISTS cineprime
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE cineprime;

CREATE TABLE users (
    id INT PRIMARY KEY AUTO_INCREMENT,
    full_name VARCHAR(100) NOT NULL,
    email VARCHAR(100) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    role ENUM('admin','operator') DEFAULT 'operator',
    status ENUM('active','inactive') DEFAULT 'active',
    theme_preference ENUM('dark','light') DEFAULT 'dark',
    last_login DATETIME,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE movies (
    id INT PRIMARY KEY AUTO_INCREMENT,
    title VARCHAR(200) NOT NULL,
    genre VARCHAR(50),
    duration_minutes INT,
    director VARCHAR(100),
    release_year INT,
    description TEXT,
    poster_url VARCHAR(500),
    rating DECIMAL(2,1) DEFAULT 0.0,
    total_reviews INT DEFAULT 0,
    status ENUM('active','upcoming','archived') DEFAULT 'active',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE halls (
    id INT PRIMARY KEY AUTO_INCREMENT,
    name VARCHAR(50) NOT NULL,
    capacity INT NOT NULL,
    rows_count INT,
    seats_per_row INT,
    hall_type ENUM('standard','vip','imax') DEFAULT 'standard',
    status ENUM('active','maintenance','inactive') DEFAULT 'active'
);

CREATE TABLE schedules (
    id INT PRIMARY KEY AUTO_INCREMENT,
    movie_id INT NOT NULL,
    hall_id INT NOT NULL,
    start_time DATETIME NOT NULL,
    end_time DATETIME NOT NULL,
    ticket_price DECIMAL(10,2) NOT NULL,
    vip_price DECIMAL(10,2),
    status ENUM('scheduled','cancelled','completed') DEFAULT 'scheduled',
    FOREIGN KEY (movie_id) REFERENCES movies(id),
    FOREIGN KEY (hall_id) REFERENCES halls(id)
);

CREATE TABLE reservations (
    id INT PRIMARY KEY AUTO_INCREMENT,
    schedule_id INT NOT NULL,
    customer_name VARCHAR(100) NOT NULL,
    customer_phone VARCHAR(20),
    customer_email VARCHAR(100),
    total_amount DECIMAL(10,2) NOT NULL,
    status ENUM('pending','confirmed','cancelled') DEFAULT 'pending',
    created_by INT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (schedule_id) REFERENCES schedules(id),
    FOREIGN KEY (created_by) REFERENCES users(id)
);

CREATE TABLE reservation_seats (
    id INT PRIMARY KEY AUTO_INCREMENT,
    reservation_id INT NOT NULL,
    seat_row INT NOT NULL,
    seat_number INT NOT NULL,
    seat_type ENUM('standard','vip') DEFAULT 'standard',
    FOREIGN KEY (reservation_id) REFERENCES reservations(id)
);

CREATE TABLE products (
    id INT PRIMARY KEY AUTO_INCREMENT,
    name VARCHAR(100) NOT NULL,
    category ENUM('snacks','drinks','food','combo') DEFAULT 'snacks',
    price DECIMAL(10,2) NOT NULL,
    stock_quantity INT DEFAULT 0,
    min_stock_alert INT DEFAULT 5,
    status ENUM('active','out_of_stock','discontinued') DEFAULT 'active'
);

CREATE TABLE sales (
    id INT PRIMARY KEY AUTO_INCREMENT,
    sale_type ENUM('tickets','products','mixed') NOT NULL,
    reservation_id INT,
    total_amount DECIMAL(10,2) NOT NULL,
    payment_method ENUM('cash','card') DEFAULT 'cash',
    created_by INT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (reservation_id) REFERENCES reservations(id),
    FOREIGN KEY (created_by) REFERENCES users(id)
);

CREATE TABLE sale_items (
    id INT PRIMARY KEY AUTO_INCREMENT,
    sale_id INT NOT NULL,
    product_id INT,
    quantity INT DEFAULT 1,
    unit_price DECIMAL(10,2) NOT NULL,
    total_price DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (sale_id) REFERENCES sales(id),
    FOREIGN KEY (product_id) REFERENCES products(id)
);

INSERT INTO users (full_name, email, password_hash, role) VALUES
('Admin User', 'admin@cineprime.md', 'hash_aici', 'admin'),
('Operator Test', 'operator@cineprime.md', 'hash_aici', 'operator');

INSERT INTO halls (name, capacity, rows_count, seats_per_row, hall_type) VALUES
('Sala 1', 120, 10, 12, 'standard'),
('Sala VIP', 40, 5, 8, 'vip');

INSERT INTO products (name, category, price, stock_quantity) VALUES
('Large Popcorn', 'snacks', 80.00, 50),
('Medium Popcorn', 'snacks', 60.00, 30),
('Small Popcorn', 'snacks', 40.00, 15),
('Large Drink', 'drinks', 50.00, 67);
