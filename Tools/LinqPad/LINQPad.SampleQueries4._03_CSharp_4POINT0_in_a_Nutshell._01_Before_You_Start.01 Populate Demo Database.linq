<Query Kind="SQL">
  <Connection>
    <ID>f2f6e2b4-0f40-48df-b2c7-82584a77c393</ID>
    <AttachFileName>&lt;ApplicationData&gt;\LINQPad\Nutshell.mdf</AttachFileName>
    <Server>.\SQLEXPRESS</Server>
    <AttachFile>true</AttachFile>
    <UserInstance>true</UserInstance>
    <Persist>true</Persist>
  </Connection>
</Query>

-- Run this script at any time, either to:
--   (a) Create the demo tables in a different database (see note in previous example)
--   (b) Restore the demo tables to their original state

if exists (select * from sysobjects where name = 'PurchaseItem') drop table PurchaseItem
if exists (select * from sysobjects where name = 'Purchase') drop table Purchase
if exists (select * from sysobjects where name = 'Customer') drop table Customer
if exists (select * from sysobjects where name = 'MedicalArticles') drop table MedicalArticles
if exists (select * from sysobjects where name = 'Product') drop table Product
go

create table Customer
(
	ID int not null primary key,
	Name nvarchar(30) not null
)

create table Purchase
(
	ID int not null primary key,
	CustomerID int null references Customer (ID),
	Date datetime not null,
	Description varchar(30) not null,
	Price decimal not null
)

create table PurchaseItem
(
	ID int not null primary key,
	PurchaseID int not null references Purchase (ID),
	Detail varchar(30) not null,
	Price decimal not null
)

create table MedicalArticles
(
	ID int not null primary key,
	Topic varchar (20),
	Abstract nvarchar (2000)	
)

create table Product
(
	ID int not null primary key,
	Description varchar(30) not null,
  	Discontinued bit not null,
  	LastSale datetime not null
)
go

insert Customer values (1, 'Tom')
insert Customer values (2, 'Dick')
insert Customer values (3, 'Harry')
insert Customer values (4, 'Mary')
insert Customer values (5, 'Jay')

insert Purchase values (1, 1, '2006-1-1', 'Bike', 500)
insert Purchase values (2, 1, '2006-1-2', 'Holiday', 2000)
insert Purchase values (3, 2, '2007-1-3', 'Bike', 600)
insert Purchase values (4, 2, '2007-1-4', 'Phone', 300)
insert Purchase values (5, 3, '2007-1-5', 'Hat', 50)
insert Purchase values (6, 4, '2008-1-6', 'Car', 15000)
insert Purchase values (7, 4, '2008-1-7', 'Boat', 30000)
insert Purchase values (8, 4, '2008-1-8', 'Camera', 1200)
insert Purchase values (9, null, '2008-1-9', 'Jacket', 80)

insert PurchaseItem values (1, 2, 'Flight', 1500)
insert PurchaseItem values (2, 2, 'Accommodation', 500)
insert PurchaseItem values (3, 2, 'Camera', 400)

insert MedicalArticles values (1, 'Influenza', '<this is the abstract...>')
insert MedicalArticles values (2, 'Diabetes', '<this is the abstract...>')

insert Product values (1, 'Widget', 0, '2007-1-1')

print 'Done!'