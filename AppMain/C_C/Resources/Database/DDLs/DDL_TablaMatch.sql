USE [C_CBD];
GO

CREATE TABLE Match(
	[ID_Match] INT IDENTITY (50001,1) PRIMARY KEY NOT NULL,
	[Perfil_Emisor] INT NOT NULL,
	[Perfil_Receptor] INT NOT NULL,
	[Fecha_Match] DATE NOT NULL,
	[Estado] NVARCHAR(50) NOT NULL CHECK (ESTADO IN ('Pendiente', 'Aceptado', 'Rechazado')),

	CONSTRAINT FK_Match_Emisor 
		FOREIGN KEY (Perfil_Emisor)
		REFERENCES Perfil(ID_Perfil)
		ON DELETE CASCADE
		ON UPDATE CASCADE,

	CONSTRAINT CHK_Perfiles_Distintos CHECK (Perfil_Emisor <> Perfil_Receptor));
	GO