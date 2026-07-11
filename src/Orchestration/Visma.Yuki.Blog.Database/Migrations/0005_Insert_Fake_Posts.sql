DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM posts LIMIT 1) THEN

        WITH author_lookups AS (
            SELECT id, name, surname FROM authors 
            WHERE uniquenameidentifier IN (
                '9bc70138988276f57849e7b4588523b092f6da1c6e1ca87869', -- John Doe
                '8fb1b1516f1a8cc3f5e5b3f2ec20fa52b4742718fae471fa28', -- Sarah Jenkins
                '13f28cf080df7f2923b03657ff64ffc93da4c2f82fc46231d6'  -- Hiroshi Tanaka
            )
        )
        INSERT INTO posts (id, title, description, content, authorid, createdat, updatedat)
        SELECT * FROM (
            VALUES 
            (
                gen_random_uuid(),
                'Unlocking High Performance in .NET with NpgsqlDataSource',
                'Learn how to optimize your PostgreSQL connection pooling in modern .NET applications.',
                'Database connection pooling is often a silent performance killer. In modern .NET applications, using NpgsqlDataSource provides a robust, thread-safe factory that drastically reduces connection overhead. In this article, we explore how to leverage it alongside Dapper...',
                (SELECT id FROM author_lookups WHERE name = 'John' AND surname = 'Doe'),
                CURRENT_TIMESTAMP - INTERVAL '2 days', -- Compatível com TIMESTAMP WITH TIME ZONE
                CURRENT_TIMESTAMP - INTERVAL '2 days'
            ),
            (
                gen_random_uuid(),
                'Designing Clean Architectures: Ports and Adapters Demystified',
                'An in-depth look at isolating your domain logic using Hexagonal Architecture.',
                'Software engineering is all about managing boundaries. Hexagonal Architecture (or Ports and Adapters) allows developers to isolate core domain logic from framework-specific details. By keeping your domain pure and communication abstracted through application ports, testing becomes seamless...',
                (SELECT id FROM author_lookups WHERE name = 'Sarah' AND surname = 'Jenkins'),
                CURRENT_TIMESTAMP - INTERVAL '1 day',
                CURRENT_TIMESTAMP - INTERVAL '1 day'
            ),
            (
                gen_random_uuid(),
                'A Deep Dive into ACID Compliance and Unit of Work with Dapper',
                'Mastering manual transaction lifecycles using Dapper micro-ORM.',
                'While object-relational mappers like Entity Framework handle transaction lifecycles automatically, micro-ORMs like Dapper require manual control. Implementing a custom Unit of Work pattern using DbTransaction ensures atomic operations across multiple repositories while maintaining optimal raw-query execution speeds...',
                (SELECT id FROM author_lookups WHERE name = 'Hiroshi' AND surname = 'Tanaka'),
                CURRENT_TIMESTAMP,
                CURRENT_TIMESTAMP
            )
        ) AS v(id, title, description, content, authorid, createdat, updatedat)
        WHERE (SELECT COUNT(*) FROM author_lookups) = 3;

    END IF;
END $$;