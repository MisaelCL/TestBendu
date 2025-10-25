USE [C_CBD];
GO

CREATE TABLE Preferencias_Carrera(
	[ID_PreferenciasCarrera] INT IDENTITY (1,1) PRIMARY KEY NOT NULL,
	[Nombre_Carrera] NVARCHAR (50) NOT NULL,
	[ID_Preferencias] INT NOT NULL,

	CONSTRAINT FK_PreferenciasCarrera_Preferencias
		FOREIGN KEY (ID_Preferencias)
		REFERENCES Perfil(ID_Perfil)
		ON DELETE CASCADE
		ON UPDATE CASCADE
		);
GO