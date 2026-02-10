pipeline {
  agent { label 'dotnet' } // your docker-capable agent node label

  environment {
    DOTNET_CLI_TELEMETRY_OPTOUT = '1'
    DOTNET_NOLOGO = '1'
  }

  stages {
    stage('Checkout') {
      steps { checkout scm }
    }

    stage('Restore') {
      steps {
        script {
          docker.image('mcr.microsoft.com/dotnet/sdk:8.0').inside {
            sh 'dotnet restore'
          }
        }
      }
    }

    stage('Build') {
      steps {
        script {
          docker.image('mcr.microsoft.com/dotnet/sdk:8.0').inside {
            sh 'dotnet build -c Release --no-restore'
          }
        }
      }
    }

    stage('Test') {
      steps {
        script {
          docker.image('mcr.microsoft.com/dotnet/sdk:8.0').inside {
            sh 'dotnet test -c Release --no-build --logger "trx;LogFileName=test_results.trx" --results-directory TestResults'
          }
        }
      }
      post {
        always {
          archiveArtifacts artifacts: 'TestResults/**', fingerprint: true
        }
      }
    }
  }
}