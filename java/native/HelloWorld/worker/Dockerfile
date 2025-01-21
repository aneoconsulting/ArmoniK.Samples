
# First Stage: Build the application
FROM maven:3.9.9-eclipse-temurin-17-alpine AS build
WORKDIR /build

# Copy only the POM first to leverage Docker cache
COPY pom.xml .

# Copy the local JAR file 
COPY target/armonik-java-0.1.0.jar /tmp/armonik-java-0.1.0.jar

# Install the local dependency
RUN mvn install:install-file \
    -Dfile=/tmp/armonik-java-0.1.0.jar \
    -DgroupId=org.aneo \
    -DartifactId=armonik-java \
    -Dversion=0.1.0 \
    -Dpackaging=jar

# Copy the rest of the source code
COPY src ./src

# Build the application
RUN mvn clean package -DskipTests

# Second Stage: Runtime environment
FROM eclipse-temurin:21-jre-noble
RUN groupadd --gid 5000 armonikuser && useradd --home-dir /home/armonikuser --create-home --uid 5000 --gid 5000 --shell /bin/sh --skel /dev/null armonikuser
RUN mkdir /cache /local_storage && chown armonikuser: /cache /local_storage
USER armonikuser
WORKDIR /app
COPY --from=build /build/target/java-samples-1.0-SNAPSHOT-shaded.jar /app/worker.jar

# Run the application
ENTRYPOINT ["java", "-jar", "/app/worker.jar"]