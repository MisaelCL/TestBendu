USE [C_CBD];
GO

CREATE TABLE Cuenta(
	[ID_Cuenta] INT IDENTITY (1001,1) NOT NULL,
	[Hash_Contraseña] NVARCHAR (50) NOT NULL,
	[Estado_Cuenta] BIT NOT NULL,
	[Fecha_Registro] DATE NOT NULL,
	[Matricula] INT PRIMARY KEY,
	CONSTRAINT FK_Cuenta_Alumno
		FOREIGN KEY (Matricula)
		REFERENCES Alumno(Matricula)
		ON DELETE CASCADE
		ON UPDATE CASCADE);
	GO