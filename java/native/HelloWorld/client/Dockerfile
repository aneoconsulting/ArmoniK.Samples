# Use the OpenJDK image as the base image
FROM openjdk:24-slim

# Set the working directory inside the container
WORKDIR /app

# Copy the application JAR file into the container
COPY target/java-client-sample-0.1.0.jar /app/client.jar

# Run the application
ENTRYPOINT ["java", "-jar", "/app/client.jar"]