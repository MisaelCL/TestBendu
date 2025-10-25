USE [C_CBD];
GO

CREATE TABLE Intereses_Buscados(
	[ID_InteresesBuscados] INT IDENTITY (1,1) PRIMARY KEY NOT NULL,
	[Nombre_Interes] NVARCHAR (50) NOT NULL,
	[ID_Preferencias] INT NOT NULL,

	CONSTRAINT FK_InteresesBuscados_Preferencias
		FOREIGN KEY (ID_Preferencias)
		REFERENCES Perfil(ID_Perfil)
		ON DELETE CASCADE
		ON UPDATE CASCADE
		);
GO