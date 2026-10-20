-- Este script recrea el esquema; elimina las tablas existentes y sus datos.
DROP TABLE IF EXISTS "PRQIngresoAutomoviles" CASCADE;
DROP TABLE IF EXISTS "PRQParqueo" CASCADE;
DROP TABLE IF EXISTS "PRQAutomoviles" CASCADE;

DROP SEQUENCE IF EXISTS "PRQIngresoAutomoviles_consecutive_seq";
DROP SEQUENCE IF EXISTS "PRQParqueo_id_seq";
DROP SEQUENCE IF EXISTS "PRQAutomoviles_id_seq";
DROP TYPE IF EXISTS "PRQTipoAutomovil";

CREATE TYPE "PRQTipoAutomovil" AS ENUM ('sedan', '4x4', 'moto');

CREATE SEQUENCE IF NOT EXISTS "PRQAutomoviles_id_seq" AS BIGINT START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE IF NOT EXISTS "PRQParqueo_id_seq" AS BIGINT START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE IF NOT EXISTS "PRQIngresoAutomoviles_consecutive_seq" AS BIGINT START WITH 1 INCREMENT BY 1;

CREATE TABLE IF NOT EXISTS "PRQAutomoviles" (
    id BIGINT NOT NULL DEFAULT nextval('"PRQAutomoviles_id_seq"'::regclass),
    color VARCHAR(40) NOT NULL,
    year INTEGER NOT NULL,
    manufacturer VARCHAR(100) NOT NULL,
    type "PRQTipoAutomovil" NOT NULL,
    CONSTRAINT prq_automoviles_pk PRIMARY KEY (id),
    CONSTRAINT prq_automoviles_color_not_blank CHECK (BTRIM(color) <> ''),
    CONSTRAINT prq_automoviles_year_valid CHECK (year BETWEEN 1886 AND EXTRACT(YEAR FROM CURRENT_DATE)::INTEGER),
    CONSTRAINT prq_automoviles_manufacturer_not_blank CHECK (BTRIM(manufacturer) <> '')
);
ALTER SEQUENCE "PRQAutomoviles_id_seq" OWNED BY "PRQAutomoviles".id;

CREATE TABLE IF NOT EXISTS "PRQParqueo" (
    id BIGINT NOT NULL DEFAULT nextval('"PRQParqueo_id_seq"'::regclass),
    province_name VARCHAR(100) NOT NULL,
    name VARCHAR(120) NOT NULL,
    price_per_hour NUMERIC(10, 2) NOT NULL,
    CONSTRAINT prq_parqueo_pk PRIMARY KEY (id),
    CONSTRAINT prq_parqueo_province_not_blank CHECK (BTRIM(province_name) <> ''),
    CONSTRAINT prq_parqueo_name_not_blank CHECK (BTRIM(name) <> ''),
    CONSTRAINT prq_parqueo_price_nonnegative CHECK (price_per_hour >= 0)
);
ALTER SEQUENCE "PRQParqueo_id_seq" OWNED BY "PRQParqueo".id;

CREATE TABLE IF NOT EXISTS "PRQIngresoAutomoviles" (
    consecutive BIGINT NOT NULL DEFAULT nextval('"PRQIngresoAutomoviles_consecutive_seq"'::regclass),
    parking_id BIGINT NOT NULL,
    car_id BIGINT NOT NULL,
    entry_datetime TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    exit_datetime TIMESTAMP WITHOUT TIME ZONE NULL,
    CONSTRAINT prq_ingreso_automoviles_pk PRIMARY KEY (consecutive),
    CONSTRAINT prq_ingreso_parking_fk FOREIGN KEY (parking_id)
        REFERENCES "PRQParqueo" (id) ON UPDATE CASCADE ON DELETE RESTRICT,
    CONSTRAINT prq_ingreso_car_fk FOREIGN KEY (car_id)
        REFERENCES "PRQAutomoviles" (id) ON UPDATE CASCADE ON DELETE RESTRICT,
    CONSTRAINT prq_ingreso_exit_after_entry CHECK (
        exit_datetime IS NULL OR exit_datetime >= entry_datetime
    )
);
ALTER SEQUENCE "PRQIngresoAutomoviles_consecutive_seq" OWNED BY "PRQIngresoAutomoviles".consecutive;
