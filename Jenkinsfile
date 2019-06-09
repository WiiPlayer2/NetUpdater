pipeline {
    agent {
        label 'windows'
    }

    stages {
        stage ('Checkout') {
            steps {
                checkout scm
            }
        }

        stage ('Cleanup') {
            steps {
                powershell './build.ps1 -Target Clean'
            }
        }

        stage ('Build') {
            steps {
                powershell './build.ps1 -Target Pack -Configuration Release'
                powershell './build.ps1 -Target Pack -Configuration Debug'
            }
        }

        stage ('Test') {
            steps {
                powershell './build.ps1 -Target Test -Configuration Release'
            }
        }
        
        stage ('Archive') {
            steps {
                archiveArtifacts '/_build/**/*.zip'
            }
        }
    }

    post {
        always {
            mstest testResultsFile: '_build/Release/*.trx'
        }
    }
}
