
# Description

This is improved version of msdn's example (https://docs.microsoft.com/en-us/azure/event-grid/resize-images-on-storage-blob-upload-event)
MSDN's version uses 2 types of bindings: eventGridTrigger and blob. I removed redundant blob binding, retrive file stream from a eventGridTrigger variable and get a file stream using connetion to the storage account and file name.
