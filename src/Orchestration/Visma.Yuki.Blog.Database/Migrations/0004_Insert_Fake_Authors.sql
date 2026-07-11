DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM authors LIMIT 1) THEN
        
        INSERT INTO authors (id, uniquenameidentifier, name, surname)
        VALUES 
        (gen_random_uuid(), '9bc70138988276f57849e7b4588523b092f6da1c6e1ca87869', 'John', 'Doe'),
        (gen_random_uuid(), '8fb1b1516f1a8cc3f5e5b3f2ec20fa52b4742718fae471fa28', 'Sarah', 'Jenkins'),
        (gen_random_uuid(), '13f28cf080df7f2923b03657ff64ffc93da4c2f82fc46231d6', 'Hiroshi', 'Tanaka');

    END IF;
END $$;