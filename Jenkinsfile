pipeline {
  agent { label 'docker-dotnet' }

  options {
    timestamps()
    disableConcurrentBuilds()
    skipDefaultCheckout(true)
  }

  environment {
    // Unique name per build so parallel builds don't collide on networks/containers
    COMPOSE_PROJECT_NAME = "ci-${JOB_NAME}".toLowerCase().replaceAll(/[^a-z0-9]+/, '-') + "-${BUILD_NUMBER}"
    IMAGE_LOCAL = "local/${JOB_NAME}".toLowerCase().replaceAll(/[^a-z0-9_.-]+/, '-') + ":${GIT_COMMIT?.take(8) ?: BUILD_NUMBER}"
  }

  stages {
    stage('Checkout') {
      steps { checkout scm }
    }

    stage('Unit Tests') {
      steps {
        sh '''
          dotnet restore
          dotnet test -c Release --logger "trx;LogFileName=test-results.trx"
        '''
      }
    }

    stage('Dockerfile Build') {
      steps {
        sh '''
          docker version
          docker build --pull -t "$IMAGE_LOCAL" .
        '''
      }
    }

    stage('Compose Validate') {
      steps {
        sh '''
          docker compose -f docker-compose.yml -f docker-compose.ci.yml config >/dev/null
        '''
      }
    }

    stage('Compose Build (ensures compose builds too)') {
      steps {
        sh '''
          docker compose -f docker-compose.yml -f docker-compose.ci.yml build --pull
        '''
      }
    }
  }

  post {
    always {
      sh '''
        docker compose -f docker-compose.yml -f docker-compose.ci.yml down -v --remove-orphans || true
        docker image rm -f "$IMAGE_LOCAL" || true
      '''
      deleteDir()
    }
  }
}
