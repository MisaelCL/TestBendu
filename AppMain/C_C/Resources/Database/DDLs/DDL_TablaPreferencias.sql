USE [C_CBD];
GO

CREATE TABLE Preferencias(
	[ID_Preferencias] INT IDENTITY (1,1) PRIMARY KEY NOT NULL,
	[Preferencias_Genero] NVARCHAR (20) NOT NULL,
	[Edad_Minima] INT NOT NULL CHECK (Edad_Minima >= 18),
	[Edad_Maxima] INT NOT NULL,
	[ID_Perfil] INT UNIQUE NOT NULL,

	CONSTRAINT CHK_Rango_Edad CHECK (Edad_Maxima >= Edad_Minima),

	CONSTRAINT FK_Preferencias_Perfil
		FOREIGN KEY (ID_Perfil)
		REFERENCES Perfil(ID_Perfil)
		ON DELETE CASCADE
		ON UPDATE CASCADE);

	GO