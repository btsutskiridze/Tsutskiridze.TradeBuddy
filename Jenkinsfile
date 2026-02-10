pipeline {
  agent {
    docker {
      image 'mcr.microsoft.com/dotnet/sdk:8.0'
      label 'dotnet'
      reuseNode true
      // args '-u 1000:1000'   // optional: avoid root-owned files (set to your agent user id)
    }
  }

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
        sh 'dotnet restore'
      }
    }

    stage('Build') {
      steps {
        sh 'dotnet build -c Release --no-restore'
      }
    }

    stage('Test') {
      steps {
        sh 'dotnet test -c Release --no-build --logger "trx;LogFileName=test_results.trx" --results-directory TestResults'
      }
      post {
        always {
          archiveArtifacts artifacts: 'TestResults/**', fingerprint: true
        }
      }
    }
  }
}