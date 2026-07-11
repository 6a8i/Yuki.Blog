CREATE INDEX IX_posts_authorId ON posts(authorId);
CREATE INDEX IX_authors_uniqueNameIdentifier ON authors(uniqueNameIdentifier);
CREATE INDEX IX_posts_createdAt_Desc ON posts(createdAt DESC);
CREATE INDEX IX_posts_title ON posts(title);