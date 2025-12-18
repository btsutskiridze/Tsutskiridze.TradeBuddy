pipeline {
  agent { label 'docker-dotnet' }

  options {
    timestamps()
    disableConcurrentBuilds()
    skipDefaultCheckout(true)
  }

  environment {
    COMPOSE_PROJECT_NAME = "ci-${JOB_NAME}".toLowerCase().replaceAll(/[^a-z0-9]+/, '-') + "-${BUILD_NUMBER}"
  }

  stages {
    stage('Checkout') {
      steps { checkout scm }
    }

    stage('Create .env.ci from Jenkins credential') {
      steps {
        withCredentials([file(credentialsId: 'tradebuddy-env-ci', variable: 'ENVFILE')]) {
          sh '''
            set -e
            cp "$ENVFILE" .env.ci
            chmod 600 .env.ci
          '''
        }
      }
    }

    stage('Compose Validate') {
      steps {
        sh 'docker compose -f docker-compose.yml -f docker-compose.ci.yml config >/dev/null'
      }
    }

    stage('Compose Build') {
      steps {
        sh 'docker compose -f docker-compose.yml -f docker-compose.ci.yml build --pull'
      }
    }

    post {
      always {
        sh '''
          docker compose -f docker-compose.yml -f docker-compose.ci.yml down -v --remove-orphans || true
          rm -f .env.ci || true
        '''
        deleteDir()
      }
    }
  }
}
