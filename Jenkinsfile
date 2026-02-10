pipeline {
  agent {
    docker {
      image 'mcr.microsoft.com/dotnet/sdk:8.0'
      label 'dotnet'
      reuseNode true
    }
  }

  environment {
    DOTNET_CLI_TELEMETRY_OPTOUT = '1'
    DOTNET_NOLOGO = '1'

    // optional but recommended: persist NuGet cache in workspace (faster builds)
    NUGET_PACKAGES = "${WORKSPACE}/.nuget/packages"
  }

  stages {
    stage('Checkout') {
      steps { checkout scm }
    }

    stage('Restore') {
      steps { sh 'dotnet restore' }
    }

    stage('Build') {
      steps { sh 'dotnet build -c Release --no-restore' }
    }

    stage('Test + Coverage') {
      steps {
        sh '''
          dotnet test -c Release --no-build \
            --logger "trx;LogFileName=test_results.trx" \
            --results-directory TestResults \
            --collect:"XPlat Code Coverage"
        '''
      }

      post {
        always {
          // 1) Publish TRX test results in Jenkins UI (requires MSTest plugin)
          // testResultsFile is supported by the mstest pipeline step :contentReference[oaicite:2]{index=2}
          mstest testResultsFile: 'TestResults/**/*.trx', failOnError: false

          // 2) Generate coverage HTML (Cobertura XML is produced under TestResults) :contentReference[oaicite:3]{index=3}
          sh '''
            set -e

            mkdir -p .tools CoverageReport

            # install ReportGenerator locally (tool-path avoids PATH issues in containers) :contentReference[oaicite:4]{index=4}
            dotnet tool install dotnet-reportgenerator-globaltool --tool-path .tools || true

            # generate HTML report from any coverage.cobertura.xml files
            ./.tools/reportgenerator \
              "-reports:TestResults/**/coverage.cobertura.xml" \
              "-targetdir:CoverageReport" \
              "-reporttypes:HtmlInline;Cobertura" || true
          '''

          // Publish coverage HTML in Jenkins UI (requires HTML Publisher plugin) :contentReference[oaicite:5]{index=5}
          publishHTML(target: [
            allowMissing: true,
            alwaysLinkToLastBuild: true,
            keepAll: true,
            reportDir: 'CoverageReport',
            reportFiles: 'index.html',
            reportName: 'Code Coverage'
          ])

          // Keep raw files too
          archiveArtifacts artifacts: 'TestResults/**,CoverageReport/**', fingerprint: true
        }
      }
    }
  }
}