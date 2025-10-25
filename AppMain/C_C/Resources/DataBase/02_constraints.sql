IF OBJECT_ID('dbo.[Match]','U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_Match_Par')
    BEGIN
        ALTER TABLE dbo.[Match] WITH CHECK
        ADD CONSTRAINT UQ_Match_Par UNIQUE (Perfil_Emisor, Perfil_Receptor);
    END;

    IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Match_Distintos')
    BEGIN
        ALTER TABLE dbo.[Match] WITH CHECK
        ADD CONSTRAINT CK_Match_Distintos CHECK (Perfil_Emisor <> Perfil_Receptor);
    END;
END;

IF OBJECT_ID('dbo.Preferencias','U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Pref_Edades')
    BEGIN
        ALTER TABLE dbo.Preferencias WITH CHECK
        ADD CONSTRAINT CK_Pref_Edades CHECK (Edad_Minima >= 18 AND Edad_Maxima <= 99 AND Edad_Minima <= Edad_Maxima);
    END;

    IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Pref_Genero')
    BEGIN
        ALTER TABLE dbo.Preferencias WITH CHECK
        ADD CONSTRAINT CK_Pref_Genero CHECK (Preferencia_Genero IN (0,1,2,3));
    END;

    IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Pref_Radio')
    BEGIN
        ALTER TABLE dbo.Preferencias WITH CHECK
        ADD CONSTRAINT CK_Pref_Radio CHECK (RadioKm BETWEEN 1 AND 1000);
    END;
END;

IF OBJECT_ID('dbo.Intereses_Buscados','U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Intereses_Def')
    BEGIN
        ALTER TABLE dbo.Intereses_Buscados WITH CHECK
        ADD CONSTRAINT CK_Intereses_Def CHECK (
            (ID_Interes IS NOT NULL AND Nombre_Interes IS NULL) OR
            (ID_Interes IS NULL AND Nombre_Interes IS NOT NULL)
        );
    END;
END;
