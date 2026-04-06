# clustering_reference.R
# Reference calculations for BESHStatNG clustering tests.
# Uses only base R / stats.

options(digits = 17)

basic <- read.csv("cluster_dataset_basic.csv", stringsAsFactors = FALSE)
missing_df <- read.csv("cluster_dataset_missing.csv", stringsAsFactors = FALSE)
complete_df <- na.omit(missing_df)

x_basic <- as.matrix(basic[, c("x1", "x2")])
rownames(x_basic) <- basic$row_label
x_complete <- as.matrix(complete_df[, c("x1", "x2")])
rownames(x_complete) <- complete_df$row_label

cat("=== K-means with user-specified centers ===\n")
km <- kmeans(x_basic,
             centers = matrix(c(0, 0,
                                10, 10), nrow = 2, byrow = TRUE),
             iter.max = 100,
             algorithm = "Lloyd")
print(km$cluster)
print(km$centers)
print(km$withinss)
print(km$tot.withinss)
print(km$betweenss)
print(km$totss)

cat("\n=== K-means with listwise deletion of missing rows ===\n")
km_missing <- kmeans(x_complete,
                     centers = matrix(c(0, 0,
                                        10, 10), nrow = 2, byrow = TRUE),
                     iter.max = 100,
                     algorithm = "Lloyd")
print(complete_df$row_label)
print(km_missing$cluster)
print(km_missing$centers)
print(km_missing$tot.withinss)

cat("\n=== Hierarchical clustering: complete linkage, Euclidean ===\n")
hc <- hclust(dist(x_basic, method = "euclidean"), method = "complete")
print(hc$merge)
print(hc$height)
print(cutree(hc, k = 2))
print(cutree(hc, h = 4.0))

cat("\n=== Hierarchical clustering with listwise deletion of missing rows ===\n")
hc_missing <- hclust(dist(x_complete, method = "euclidean"), method = "complete")
print(complete_df$row_label)
print(hc_missing$merge)
print(hc_missing$height)
print(cutree(hc_missing, k = 2))
print(cutree(hc_missing, h = 4.0))
