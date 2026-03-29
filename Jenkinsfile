pipeline {
    agent any

    environment {
        DOCKERHUB_CREDENTIALS = credentials('dockerhub-credentials')
        DOCKERHUB_USERNAME    = 'raveenadev'
        BACKEND_IMAGE         = 'raveenadev/elearning-backend'
        FRONTEND_IMAGE        = 'raveenadev/elearning-frontend'
        IMAGE_TAG             = "${BUILD_NUMBER}"
    }

    stages {

        stage('Checkout') {
            steps {
                echo '📥 Checking out source code...'
                checkout scm
            }
        }

        stage('Build Backend') {
            steps {
                echo '🔨 Building backend Docker image...'
                dir('E-learning Portal') {
                    sh """
                        docker build \
                            -t ${BACKEND_IMAGE}:${IMAGE_TAG} \
                            -t ${BACKEND_IMAGE}:latest \
                            .
                    """
                }
            }
        }

        stage('Build Frontend') {
            steps {
                echo '🔨 Building frontend Docker image...'
                dir('elearning-portal') {
                    sh """
                        docker build \
                            -t ${FRONTEND_IMAGE}:${IMAGE_TAG} \
                            -t ${FRONTEND_IMAGE}:latest \
                            .
                    """
                }
            }
        }

        stage('Push to Docker Hub') {
            steps {
                echo '📤 Pushing images to Docker Hub...'
                sh """
                    echo ${DOCKERHUB_CREDENTIALS_PSW} | \
                    docker login -u ${DOCKERHUB_CREDENTIALS_USR} --password-stdin
                """
                sh "docker push ${BACKEND_IMAGE}:${IMAGE_TAG}"
                sh "docker push ${BACKEND_IMAGE}:latest"
                sh "docker push ${FRONTEND_IMAGE}:${IMAGE_TAG}"
                sh "docker push ${FRONTEND_IMAGE}:latest"
            }
        }

        stage('Deploy') {
            steps {
                echo '🚀 Deploying updated containers...'
                sh """
                    docker pull ${BACKEND_IMAGE}:latest
                    docker pull ${FRONTEND_IMAGE}:latest

                    docker stop elearning-backend  || true
                    docker stop elearning-frontend || true
                    docker rm   elearning-backend  || true
                    docker rm   elearning-frontend || true

                    docker run -d \
                        --name elearning-backend \
                        -p 5000:8080 \
                        -e ASPNETCORE_ENVIRONMENT=Development \
                        -e ASPNETCORE_URLS=http://+:8080 \
                        -e ConnectionStrings__DefaultConnection="Host=elearning-db;Port=5432;Database=elearningdb;Username=postgres;Password=postgres;Maximum Pool Size=20" \
                        -e Keycloak__Authority=http://host.docker.internal:9090/realms/elearning-realm \
                        -e Keycloak__PublicIssuer=http://localhost:9090/realms/elearning-realm \
                        -e Keycloak__Realm=elearning-realm \
                        -e Keycloak__AdminClientId=admin-cli \
                        -e Keycloak__AdminUsername=admin \
                        -e Keycloak__AdminPassword=admin \
                        -e Kestrel__Limits__MaxRequestBodySize=524288000 \
                        ${BACKEND_IMAGE}:latest

                    docker run -d \
                        --name elearning-frontend \
                        -p 4200:80 \
                        ${FRONTEND_IMAGE}:latest
                """
            }
        }
    }

    post {
        success {
            echo '✅ Pipeline completed successfully!'
            echo "Backend:  ${BACKEND_IMAGE}:${IMAGE_TAG}"
            echo "Frontend: ${FRONTEND_IMAGE}:${IMAGE_TAG}"
        }
        failure {
            echo '❌ Pipeline failed!'
        }
        always {
            sh 'docker logout || true'
        }
    }
}